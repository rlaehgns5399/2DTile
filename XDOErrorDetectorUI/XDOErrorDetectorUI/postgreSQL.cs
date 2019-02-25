using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using System.ComponentModel;
using System.Diagnostics;

namespace XDOErrorDetectorUI
{
    class postgreSQL
    {
        public DB info;
        private int xdoDBInsertCount = 0;
        private int xdoLogInsertCount = 0;
        private int datDBInsertCount = 0;
        private int datLogInsertCount = 0;

        public string table_xdo, table_xdo_log, table_xdo_etc;
        public string table_dat, table_dat_log, table_dat_etc;

        public List<string> search(string path, int min, int max, BackgroundWorker worker, Nullable<bool> isRepair)
        {
            var Watch = new Stopwatch();
            Watch.Restart();

            xdoDBInsertCount = 0;
            xdoLogInsertCount = 0;
            datDBInsertCount = 0;
            datLogInsertCount = 0;

            var DATdirectorySet = new DirectoryFinder(path, min, max).run(EXT.DAT);
            worker.ReportProgress(0, new ReportProgressItemClass(DATdirectorySet.Count));
            foreach (string DATFolderPath in DATdirectorySet)
            {
                var DATFileList = new FileFinder(DATFolderPath).run(EXT.DAT);

                var hashMap = new Dictionary<string, DATDBItem>();
                var xdoSet = new HashSet<string>();

                var repairDatDictionary = new Dictionary<LOG, HashSet<string>>();

                // CAN REPAIR
                repairDatDictionary[LOG.WARN_CASE_SENSITIVE] = new HashSet<string>();
                repairDatDictionary[LOG.DUPLICATE_XDO] = new HashSet<string>();
                repairDatDictionary[LOG.ERR_NOT_EXIST] = new HashSet<string>();

                // CANNOT REPAIR
                repairDatDictionary[LOG.DAT_CANNOT_PARSE_INVALID_XDONAME] = new HashSet<string>();
                repairDatDictionary[LOG.DAT_CANNOT_PARSE_NOT_EXIST_DIRECTORY] = new HashSet<string>();

                foreach (string datFile in DATFileList)
                {
                    var dat = new ReadDAT(datFile);
                    var baseDirectory = new FileInfo(datFile).Directory.FullName;

                    var dat_DBItem = new DATDBItem();

                    dat_DBItem.level = (int)dat.header.level;
                    dat_DBItem.idx = (int)dat.header.IDX;
                    dat_DBItem.idy = (int)dat.header.IDY;
                    dat_DBItem.objCount = (int)dat.header.objCount;
                    dat_DBItem.datFileName = dat.header.datFilename;
                    for (int i = 0; i < dat_DBItem.objCount; i++)
                    {
                        var version = dat.body[i].version;
                        var version_string = Int32.Parse(string.Format("{0}{1}{2}{3}", version[0], version[1], version[2], version[3]));
                        dat_DBItem.version.Add(version_string);
                        dat_DBItem.key.Add(dat.body[i].key);
                        dat_DBItem.centerPos_X.Add(dat.body[i].centerPos_x);
                        dat_DBItem.centerPos_Y.Add(dat.body[i].centerPos_y);
                        dat_DBItem.altitude.Add(dat.body[i].altitude);
                        dat_DBItem.minX.Add(dat.body[i].minX);
                        dat_DBItem.minY.Add(dat.body[i].minY);
                        dat_DBItem.minZ.Add(dat.body[i].minZ);
                        dat_DBItem.maxX.Add(dat.body[i].maxX);
                        dat_DBItem.maxY.Add(dat.body[i].maxY);
                        dat_DBItem.maxZ.Add(dat.body[i].maxZ);
                        dat_DBItem.imgLevel.Add(dat.body[i].ImgLevel);
                        dat_DBItem.dataFile.Add(dat.body[i].dataFile);
                        dat_DBItem.imgFileName.Add(dat.body[i].imgFileName);
                    }

                    hashMap.Add(datFile, dat_DBItem);

                    try
                    {
                        IEnumerable<string> temp = Directory.EnumerateFiles(Path.Combine(baseDirectory, Path.GetFileNameWithoutExtension(datFile)), "*", SearchOption.TopDirectoryOnly);
                        foreach (var xdoFile in temp)
                        {
                            if (new FileInfo(xdoFile.ToLower()).Extension.Equals(".xdo"))
                            {
                                xdoSet.Add(xdoFile);
                            }
                        }
                    }
                    catch (ArgumentException e)
                    {
                        var errorPath = Path.Combine(baseDirectory, Path.GetFileNameWithoutExtension(datFile));
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("[X] Filename Error in " + errorPath);
                        Console.ResetColor();
                        repairDatDictionary[LOG.DAT_CANNOT_PARSE_INVALID_XDONAME].Add(errorPath);
                    }
                    catch (DirectoryNotFoundException e)
                    {
                        var errorPath = Path.Combine(baseDirectory, Path.GetFileNameWithoutExtension(datFile));
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[X] DirectoryNotFound in " + errorPath);
                        Console.ResetColor();
                        repairDatDictionary[LOG.DAT_CANNOT_PARSE_NOT_EXIST_DIRECTORY].Add(errorPath);
                    }
                }

                var DATLogList = checkDATError(hashMap, xdoSet, repairDatDictionary);
                this.repair(repairDatDictionary, DATLogList, isRepair);
                writeDBwithDATinfo(hashMap, DATLogList, repairDatDictionary);

                worker.ReportProgress(1);

                /*
                 * prevent memory leaks?
                 */
                DATFileList.Clear();
                hashMap.Clear();
                xdoSet.Clear();
                repairDatDictionary.Clear();

                DATFileList = null;
                hashMap = null;
                xdoSet = null;
                repairDatDictionary = null;
            }

            Watch.Stop();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ======================= \n DAT Analysis complete \n = " + Watch.ElapsedMilliseconds / 1000 + "s =\n" + " ======================= ");
            Console.ResetColor();

            Watch.Restart();
            var XDOdirectorySet = new DirectoryFinder(path, min, max).run(EXT.XDO);

            //var XDOdirectorySet = new HashSet<string>();
            
            //foreach ( var file in Directory.EnumerateDirectories(@"Z:\building\15\00112422", "*", SearchOption.TopDirectoryOnly))
            //{
            //    XDOdirectorySet.Add(file);
            //}
            
                
            worker.ReportProgress(0, new ReportProgressItemClass(XDOdirectorySet.Count));

            foreach (string directory in XDOdirectorySet)
            {
                
                var repairXdoDictionary = new Dictionary<LOG, HashSet<RepairXDO>>();

                // CAN REPAIR
                repairXdoDictionary[LOG.WARN_CASE_SENSITIVE] = new HashSet<RepairXDO>();
                repairXdoDictionary[LOG.XDO_VERSION_ERROR] = new HashSet<RepairXDO>();
                repairXdoDictionary[LOG.ERR_NOT_EXIST] = new HashSet<RepairXDO>();

                // CANNOT REPAIR
                

                // XDO 파일을 주어진 경로로 부터 모두 다 찾음
                var xdoFileReader = new FileFinder(directory);
                var xdoFileList = xdoFileReader.run(EXT.XDO);
                // DB에 저장할 hashMap 구성
                var hashMap = new Dictionary<string, XDODBItem>();
                var imageSet = new HashSet<string>();


                var LogFromXDO = new List<XDOLogItem>();
                var LogHashSetForDuplicatedItem = new HashSet<string>();
                // XDO(v3.0.0.2) Read -> 이미지 집합 및 1 DBItem row 생성
                // 나중에 안쓰이는 그림 파일 찾을 때 쓰임
                // 비효율적으로 제작
                foreach (var xdoFile in xdoFileList)
                {
                    if (xdoFile.Equals("###"))
                    {
                        LogHashSetForDuplicatedItem.Add(directory);
                    }
                    else
                    {
                        var xdo = new ReadXDO(xdoFile, LogFromXDO);
                        if (xdo.isEnd == false) new PrintInformation(xdo);
                        var baseDirectory = new FileInfo(xdo.url).Directory.FullName;

                        var xdo_dbItem = new XDODBItem();


                        var xy = new FileInfo(xdoFile).Directory.Name;
                        var name = xy.Split('_');
                        xdo_dbItem.level = Int32.Parse(new FileInfo(xdoFile).Directory.Parent.Parent.Name);
                        xdo_dbItem.X = name[1];
                        xdo_dbItem.Y = name[0];
                        xdo_dbItem.minifiedName = new FileInfo(xdoFile).Name;
                        xdo_dbItem.XDOVersion = xdo.XDOVersion;
                        xdo_dbItem.fileName = xdoFile;
                        xdo_dbItem.ObjectID = (int)xdo.ObjectID;
                        xdo_dbItem.Key = xdo.Key;
                        xdo_dbItem.ObjBox[0] = xdo.minX;
                        xdo_dbItem.ObjBox[1] = xdo.minY;
                        xdo_dbItem.ObjBox[2] = xdo.minZ;
                        xdo_dbItem.ObjBox[3] = xdo.maxX;
                        xdo_dbItem.ObjBox[4] = xdo.maxY;
                        xdo_dbItem.ObjBox[5] = xdo.maxZ;
                        xdo_dbItem.Altitude = xdo.altitude;

                        if (xdo.XDOVersion == 1)
                        {
                            xdo_dbItem.FaceNum = 1;
                        }
                        else
                        {
                            xdo_dbItem.FaceNum = xdo.faceNum;
                        }

                        for (int i = 0; i < xdo.faceNum; i++)
                        {
                            xdo_dbItem.VertexCount.Add((int)xdo.mesh[i].vertexCount);
                            xdo_dbItem.IndexedCount.Add((int)xdo.mesh[i].indexedCount);
                            xdo_dbItem.ImageLevel.Add((int)xdo.mesh[i].ImageLevel);
                            xdo_dbItem.ImageName.Add(xdo.mesh[i].imageName);
                        }

                        hashMap.Add(xdoFile, xdo_dbItem);


                        try
                        {
                            foreach (var imgFile in Directory.EnumerateFiles(baseDirectory, "*.jpg", SearchOption.TopDirectoryOnly))
                            {
                                imageSet.Add(imgFile);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }

                var XDOLogList = checkXDOError(hashMap, imageSet, repairXdoDictionary);
                foreach (var logItem in LogFromXDO)
                    XDOLogList.Add(logItem);
                foreach (var logItem in LogHashSetForDuplicatedItem)
                {
                    var fileInfo = new FileInfo(logItem);
                    var level = fileInfo.Directory.Parent.Name;
                    var yx = fileInfo.Name.Split('_');

                    XDOLogList.Add(new XDOLogItem(level, yx[0], yx[1], LOG.INVALID_FILENAME_IN_DIRECTORY, "", 0, "", ""));
                }
                this.repair(repairXdoDictionary, XDOLogList, isRepair);
                writeDBwithXDOInfo(hashMap, XDOLogList);

                worker.ReportProgress(1);

                hashMap.Clear();
                hashMap = null;

            }

            Watch.Stop();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ======================= \n XDO Analysis complete \n = " + Watch.ElapsedMilliseconds / 1000 + "s =\n" + " ======================= ");
            Console.ResetColor();

            Console.WriteLine("[!]\tExtracting invalid naming files...");
            var result = new List<string>();
            var invalidFileString = getInvalidFileList(path);
            var resultString = "데이터 " + datDBInsertCount + "/" + datLogInsertCount + "/" + xdoDBInsertCount + "/" + xdoLogInsertCount + "개가 추가되었습니다.";

            result.Add(invalidFileString);
            result.Add(resultString);

            return result;
        }
        public List<DATLogItem> checkDATError(Dictionary<string, DATDBItem> hashMap, HashSet<string> xdoSet, Dictionary<LOG, HashSet<string>> repairDatDictionary)
        {
            var log = new List<DATLogItem>();

            foreach (KeyValuePair<string, DATDBItem> item in hashMap)
            {
                var baseURL = new FileInfo(item.Key).Directory.FullName;
                var DATName = Path.GetFileNameWithoutExtension(item.Key);

                var level = new FileInfo(item.Key).Directory.Parent.Name;
                var xy = DATName.Split('_'); // y : xy[0], x : xy[1]

                var xdoSet_forRemove = new HashSet<string>();
                
                var duplicatedReferenceList = item.Value.dataFile.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                if(duplicatedReferenceList.Count > 0)
                {
                    // xdo 중복 사용
                    foreach (string duplicatedListItem in duplicatedReferenceList)
                        log.Add(new DATLogItem(level, xy[0], xy[1], LOG.DUPLICATE_XDO, new FileInfo(item.Key).Name, duplicatedListItem, "", ""));
                    repairDatDictionary[LOG.DUPLICATE_XDO].Add(item.Key);
                }
                for(int i = 0; i < item.Value.objCount; i++)
                {
                    var targetXDOURL = baseURL + @"\" + DATName + @"\" + item.Value.dataFile[i];
                    var DAT_xdo = Path.GetFullPath(targetXDOURL);
                    var objCountString = i + "/" + item.Value.objCount;
                    if (File.Exists(targetXDOURL))
                    {
                        var REAL_xdo = Directory.GetFiles(Path.GetDirectoryName(Path.GetFullPath(targetXDOURL)), Path.GetFileName(Path.GetFullPath(targetXDOURL))).Single();
                        if (DAT_xdo == REAL_xdo)
                        {
                            // 정상
                            xdoSet_forRemove.Add(REAL_xdo);
                        }
                        else
                        {
                            // XDO 대소문자
                            log.Add(new DATLogItem(level, xy[0], xy[1], LOG.WARN_CASE_SENSITIVE, new FileInfo(item.Key).Name, new FileInfo(DAT_xdo).Name, new FileInfo(REAL_xdo).Name, objCountString));
                            xdoSet_forRemove.Add(REAL_xdo);
                            repairDatDictionary[LOG.WARN_CASE_SENSITIVE].Add(item.Key);
                        }
                    }
                    else
                    {
                        log.Add(new DATLogItem(level, xy[0], xy[1], LOG.ERR_NOT_EXIST, new FileInfo(item.Key).Name, new FileInfo(DAT_xdo).Name, "", objCountString));
                        repairDatDictionary[LOG.ERR_NOT_EXIST].Add(item.Key);
                    }
                }

                xdoSet.RemoveWhere(s => xdoSet_forRemove.Contains(s));
            }

            foreach(string xdo in xdoSet)
            {
                // 참조 안되는 XDO
                var fileinfo = new FileInfo(xdo);
                var y_x = fileinfo.Directory.Name.Split('_');
                log.Add(new DATLogItem(fileinfo.Directory.Parent.Parent.Name, y_x[0], y_x[0], LOG.NOT_USED, "", "", new FileInfo(xdo).Name, ""));
            }
            return log;
        }
        public void writeDBwithDATinfo(Dictionary<string, DATDBItem> hashMap, List<DATLogItem> log, Dictionary<LOG, HashSet<string>> repairDatDictionary)
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;

                        cmd.CommandText = "INSERT INTO " + table_dat + 
                            "(\"level\",        \"IDX\",    \"IDY\",    \"filename\",   \"ObjCount\",   \"Version\",    \"Key\",    \"CenterPos_X\",    \"CenterPos_Y\",    \"Altitude\",   \"ImageLevel\", \"dataFile\",   \"imgFileName\",    \"boxMinX\",    \"boxMinY\",    \"boxMinZ\",    \"boxMaxX\",    \"boxMaxY\",    \"boxMaxZ\")" +
                            @"VALUES(@Level,    @IDX,       @IDY,       @fileName,      @ObjCount,      @Version,       @Key,       @CenterPos_X,       @CenterPos_Y,       @Altitude,      @ImageLevel,    @dataFile,      @imgFileName,       @boxMinX,       @boxMinY,       @boxMinZ,       @boxMaxX,       @boxMaxY,       @boxMaxZ)";
                        cmd.Parameters.Add(new NpgsqlParameter("Level", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("IDX", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("IDY", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("fileName", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjCount", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("Version", NpgsqlDbType.Array | NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("Key", NpgsqlDbType.Array | NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("CenterPos_X", NpgsqlDbType.Array | NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("CenterPos_Y", NpgsqlDbType.Array | NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("Altitude", NpgsqlDbType.Array | NpgsqlDbType.Real));
                        cmd.Parameters.Add(new NpgsqlParameter("ImageLevel", NpgsqlDbType.Array | NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("dataFile", NpgsqlDbType.Array | NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("imgFileName", NpgsqlDbType.Array | NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("boxMinX", NpgsqlDbType.Array | NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("boxMinY", NpgsqlDbType.Array | NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("boxMinZ", NpgsqlDbType.Array | NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("boxMaxX", NpgsqlDbType.Array | NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("boxMaxY", NpgsqlDbType.Array | NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("boxMaxZ", NpgsqlDbType.Array | NpgsqlDbType.Double));
                        cmd.Prepare();

                        foreach (KeyValuePair<String, DATDBItem> key in hashMap)
                        {
                            Object[] obj_container = {
                                key.Value.level,
                                key.Value.idx,
                                key.Value.idy,
                                new FileInfo(key.Key).Name,
                                key.Value.objCount,
                                key.Value.version.ToArray(),
                                key.Value.key.ToArray(),
                                key.Value.centerPos_X.ToArray(),
                                key.Value.centerPos_Y.ToArray(),
                                key.Value.altitude.ToArray(),
                                key.Value.imgLevel.ToArray(),
                                key.Value.dataFile.ToArray(),
                                key.Value.imgFileName.ToArray(),
                                key.Value.minX.ToArray(),
                                key.Value.minY.ToArray(),
                                key.Value.minZ.ToArray(),
                                key.Value.maxX.ToArray(),
                                key.Value.maxY.ToArray(),
                                key.Value.maxZ.ToArray()
                            };
                            for (int i = 0; i < cmd.Parameters.Count; i++)
                                cmd.Parameters[i].Value = obj_container[i];
                            cmd.ExecuteNonQuery();
                        }

                        cmd.Parameters.Clear();

                        cmd.CommandText = "INSERT INTO " + table_dat_log + " (\"level\", \"Y\",\"X\", \"filename\", \"objCount\", \"xdoname\", \"found\", \"detail\") " +
                        @"VALUES(@level, @Y, @X, @filename, @objCount, @xdoname, @found, @detail)";
                        cmd.Parameters.Add(new NpgsqlParameter("level", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("Y", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("X", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("filename", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("objCount", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("xdoname", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("found", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("detail", NpgsqlDbType.Text));
                        cmd.Prepare();

                        foreach (DATLogItem item in log)
                        {
                            Object[] obj_log_container = {
                                item.level,
                                item.Y,
                                item.X,
                                item.filename,
                                item.objCount,
                                item.xdoname,
                                item.found,
                                item.detail
                            };
                            for (int i = 0; i < cmd.Parameters.Count; i++)
                                cmd.Parameters[i].Value = obj_log_container[i];
                            cmd.ExecuteNonQuery();
                        }

                        datDBInsertCount += hashMap.Count;
                        datLogInsertCount += log.Count;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        public List<XDOLogItem> checkXDOError(Dictionary<string, XDODBItem> hashMap, HashSet<string> imageSet, Dictionary<LOG, HashSet<RepairXDO>> repairXdoDictionary)
        {
            var log = new List<XDOLogItem>();
            foreach(KeyValuePair<string, XDODBItem> key in hashMap)
            {
                var level = new FileInfo(key.Key).Directory.Parent.Parent.Name;
                var xy = new FileInfo(key.Key).Directory.Name;
                var name = xy.Split('_');

                var imageSet_forRemove = new HashSet<string>();
                var baseURL = new FileInfo(key.Key).Directory.FullName;
                for (var i = 0; i < key.Value.FaceNum; i++)
                {
                    var imgURL = baseURL + "\\" + key.Value.ImageName[i];
                    var basecount = 0;

                    foreach (string imageSetElement in imageSet)
                    {
                        if (imgURL.Equals(imageSetElement))
                        {
                            // 성공
                            imageSet_forRemove.Add(imageSetElement);
                            basecount++;
                            break;
                        }
                        else if (imgURL.ToLower().Equals(imageSetElement.ToLower()))
                        {
                            // lower/upper Case 오류
                            log.Add(new XDOLogItem(level, name[0], name[1], LOG.WARN_CASE_SENSITIVE, new FileInfo(key.Key).Name, i, key.Value.ImageName[i], new FileInfo(imageSetElement).Name));
                            imageSet_forRemove.Add(imageSetElement);
                            repairXdoDictionary[LOG.WARN_CASE_SENSITIVE].Add(new RepairXDO(new ReadXDO(key.Key), new FileInfo(imgURL).Name));
                            basecount++;
                            break;
                        }
                    }
                    if(basecount == 0)
                    {
                        // texture가 없음
                        log.Add(new XDOLogItem(level, name[0], name[1], LOG.ERR_NOT_EXIST, new FileInfo(key.Key).Name, i, key.Value.ImageName[i], ""));
                    }


                    int imageNum = key.Value.ImageLevel[i];

                    if (key.Value.ImageLevel[i] == 1)
                    {
                        // 기본 텍스쳐도 없는 경우는 1
                        imageNum -= 1;
                    }
                    else if(key.Value.ImageLevel[i] == 2)
                    {
                        // 기본 텍스쳐만 있는경우(레벨이 없는 이미지)
                        imageNum -= 2;
                    }
                    else if(key.Value.ImageLevel[i] > 2)
                    {
                        // 여러 레벨이 있는 텍스쳐라면 2를 빼줌
                        imageNum -= 2;
                    }

                    var count = 0;

                    var lv_checker = new List<int>();
                    for (var t = 1; t <= imageNum; t++) lv_checker.Add(t);
                    for (var j = 1; j <= imageNum; j++)
                    {
                        var imgLodUrl = baseURL + "\\" + key.Value.ImageName[i].Replace(".", "_" + j + ".");
                        foreach (string imageSetElement in imageSet)
                        {
                            if (imgLodUrl.Equals(imageSetElement))
                            {
                                // 성공
                                // xdo level 체크를 위해 수를 세어야함.
                                lv_checker.Remove(j);
                                imageSet_forRemove.Add(imageSetElement);
                                count++;
                                break;
                            }
                            else if (imgLodUrl.ToLower().Equals(imageSetElement.ToLower()))
                            {
                                // lower/upper Case 오류
                                // xdo level 체크를 위해 수를 세어야함
                                lv_checker.Remove(j);
                                log.Add(new XDOLogItem(level, name[0], name[1], LOG.WARN_CASE_SENSITIVE, new FileInfo(key.Key).Name, i, new FileInfo(imgLodUrl).Name, new FileInfo(imageSetElement).Name));
                                imageSet_forRemove.Add(imageSetElement);
                                count++;
                                repairXdoDictionary[LOG.WARN_CASE_SENSITIVE].Add(new RepairXDO(new ReadXDO(key.Key), new FileInfo(Path.Combine(baseURL, key.Value.ImageName[i])).Name, j));
                                break;
                            }
                        }
                    }

                    if (count != imageNum)
                    {
                        // if(갯수가 차이가 나면) level error
                        log.Add(new XDOLogItem(level, name[0], name[1], LOG.XDO_LEVEL_ERROR, new FileInfo(key.Key).Name, i, new FileInfo(imgURL).Name.Replace(".", "_?."), "", lv_checker));
                    }
                }
                foreach(string t in imageSet_forRemove) {
                    imageSet.Remove(t);
                }
            }
            foreach(string remainImage in imageSet)
            {
                // 쓰이지 않는 것들 여기서 처리
                // 이미지 경로가 xdo와 같이 있는 "정상적인 상황"이라면 잘 동작할 것이다
                var fileinfo = new FileInfo(remainImage);
                var y_x = fileinfo.Directory.Name.Split('_');
                log.Add(new XDOLogItem(fileinfo.Directory.Parent.Parent.Name , y_x[0], y_x[1], LOG.NOT_USED, "", 0, "", new FileInfo(remainImage).Name));
            }
            return log;
        }
        public void writeDBwithXDOInfo(Dictionary<string, XDODBItem> hashMap, List<XDOLogItem> LogList)
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;

                        cmd.CommandText = "INSERT INTO " + table_xdo + " (\"Level\", \"X\", \"Y\", \"fileName\", \"ObjectID\", \"Key\", \"ObjBox_minX\", \"ObjBox_minY\", \"ObjBox_minZ\", \"ObjBox_maxX\", \"ObjBox_maxY\", \"ObjBox_maxZ\", \"Altitude\", \"FaceNum\", \"XDOVersion\", \"VertexCount\", \"IndexedCount\", \"ImageLevel\", \"ImageName\") " +
                            @"VALUES(@Level, @X, @Y, @fileName, @ObjectID, @Key, @ObjBox_minX, @ObjBox_minY, @ObjBox_minZ, @ObjBox_maxX, @ObjBox_maxY, @ObjBox_maxZ, @Altitude, @FaceNum, @XDOVersion, @VertexCount, @IndexedCount, @ImageLevel, @ImageName)";
                        cmd.Parameters.Add(new NpgsqlParameter("Level", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("X", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("Y", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("fileName", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjectID", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("Key", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_minX", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_minY", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_minZ", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_maxX", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_maxY", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("ObjBox_maxZ", NpgsqlDbType.Double));
                        cmd.Parameters.Add(new NpgsqlParameter("Altitude", NpgsqlDbType.Real));
                        cmd.Parameters.Add(new NpgsqlParameter("FaceNum", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("XDOVersion", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("VertexCount", NpgsqlDbType.Array | NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("IndexedCount", NpgsqlDbType.Array | NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("ImageLevel", NpgsqlDbType.Array | NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("ImageName", NpgsqlDbType.Array | NpgsqlDbType.Text));

                        cmd.Prepare();

                        foreach (KeyValuePair<String, XDODBItem> key in hashMap)
                        {
                            Object[] obj_container = {
                                key.Value.level,
                                key.Value.X,
                                key.Value.Y,
                                key.Value.minifiedName,
                                key.Value.ObjectID,
                                key.Value.Key,
                                key.Value.ObjBox[0],
                                key.Value.ObjBox[1],
                                key.Value.ObjBox[2],
                                key.Value.ObjBox[3],
                                key.Value.ObjBox[4],
                                key.Value.ObjBox[5],
                                key.Value.Altitude,
                                key.Value.FaceNum,
                                key.Value.XDOVersion,
                                key.Value.VertexCount.ToArray(),
                                key.Value.IndexedCount.ToArray(),
                                key.Value.ImageLevel.ToArray(),
                                key.Value.ImageName.ToArray()
                            };
                            for (int i = 0; i < cmd.Parameters.Count; i++)
                                cmd.Parameters[i].Value = obj_container[i];
                            cmd.ExecuteNonQuery();
                        }

                        cmd.Parameters.Clear();
                        cmd.CommandText = "INSERT INTO " + table_xdo_log + " (\"level\", \"Y\",\"X\", \"filename\", \"facenum\", \"imgname\", \"found\", \"detail\") " +
                            @"VALUES(@level, @Y, @X, @filename, @facenum, @imgname, @found, @detail)";
                        cmd.Parameters.Add(new NpgsqlParameter("level", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("Y", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("X", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("filename", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("facenum", NpgsqlDbType.Integer));
                        cmd.Parameters.Add(new NpgsqlParameter("imgname", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("found", NpgsqlDbType.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("detail", NpgsqlDbType.Text));
                        cmd.Prepare();

                        foreach (XDOLogItem item in LogList)
                        {
                            Object[] obj_container = {
                                item.level,
                                item.Y,
                                item.X,
                                item.filename,
                                item.facenum,
                                item.imgname,
                                item.found,
                                item.detail
                            };
                            for (int i = 0; i < cmd.Parameters.Count; i++)
                                cmd.Parameters[i].Value = obj_container[i];
                            cmd.ExecuteNonQuery();
                        }
                        xdoDBInsertCount += hashMap.Count;
                        xdoLogInsertCount += LogList.Count;

                        // Console.WriteLine(hashMap.Count + "/" + LogList.Count);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }
            }
        }

        public List<XDODBItem> loadXDOTable()
        {
            var list = new List<XDODBItem>();
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from " + table_xdo;
                        
                        using(var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new XDODBItem();
                                item.level = int.Parse(reader["level"].ToString());
                                item.X = reader["X"].ToString();
                                item.Y = reader["Y"].ToString();
                                item.minifiedName = reader["fileName"].ToString();
                                item.ObjectID = int.Parse(reader["ObjectID"].ToString());
                                item.Key = reader["Key"].ToString();
                                item.ObjBox = new double[6] {
                                    double.Parse(reader["ObjBox_minX"].ToString()),
                                    double.Parse(reader["ObjBox_minY"].ToString()),
                                    double.Parse(reader["ObjBox_minZ"].ToString()),
                                    double.Parse(reader["ObjBox_maxX"].ToString()),
                                    double.Parse(reader["ObjBox_maxY"].ToString()),
                                    double.Parse(reader["ObjBox_maxZ"].ToString())
                                };
                                item.Altitude = float.Parse(reader["Altitude"].ToString());
                                item.FaceNum = int.Parse(reader["FaceNum"].ToString());
                                item.XDOVersion = int.Parse(reader["XDOVersion"].ToString());
                                item.VertexCount = ((int[])reader["VertexCount"]).ToList();
                                item.IndexedCount = ((int[])reader["IndexedCount"]).ToList();
                                item.ImageLevel = ((int[])reader["ImageLevel"]).ToList();
                                item.ImageName = ((string[])reader["ImageName"]).ToList();
                                list.Add(item);
                            }
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return list;
        }
        public List<DATDBItem> loadDATTable()
        {
            var list = new List<DATDBItem>();
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from " + table_dat;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new DATDBItem();
                                item.level = int.Parse(reader["level"].ToString());
                                item.idx = int.Parse(reader["IDX"].ToString());
                                item.idy = int.Parse(reader["IDY"].ToString());
                                item.datFileName = reader["filename"].ToString();
                                item.objCount = int.Parse(reader["objCount"].ToString());
                                item.key = ((string[])reader["Key"]).ToList();
                                item.version = ((int[])reader["version"]).ToList();
                                item.centerPos_X = ((double[])reader["CenterPos_X"]).ToList();
                                item.centerPos_Y = ((double[])reader["CenterPos_Y"]).ToList();
                                item.altitude = ((float[])reader["Altitude"]).ToList();
                                item.imgLevel = ((int[])reader["ImageLevel"]).ToList();
                                item.dataFile = ((string[])reader["dataFile"]).ToList();
                                item.imgFileName = ((string[])reader["imgFileName"]).ToList();
                                item.minX = ((double[])reader["boxMinX"]).ToList();
                                item.minY = ((double[])reader["boxMinY"]).ToList();
                                item.minZ = ((double[])reader["boxMinZ"]).ToList();
                                item.maxX = ((double[])reader["boxMaxX"]).ToList();
                                item.maxY = ((double[])reader["boxMaxY"]).ToList();
                                item.maxZ = ((double[])reader["boxMaxZ"]).ToList();
                                list.Add(item);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return list;
        }
        public List<DATLogItem> loadDATLogTable()
        {
            var list = new List<DATLogItem>();
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from " + table_dat_log;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new DATLogItem();
                                item.level = reader["level"].ToString();
                                item.X = reader["X"].ToString();
                                item.Y = reader["Y"].ToString();
                                item.filename = reader["filename"].ToString();
                                item.found = reader["found"].ToString();
                                item.xdoname = reader["xdoname"].ToString();
                                item.detail = reader["detail"].ToString();
                                item.objCount = reader["objCount"].ToString();
                                list.Add(item);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return list;
        }
        public List<XDOLogItem> loadXDOLogTable()
        {
            var list = new List<XDOLogItem>();
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from " + table_xdo_log;

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new XDOLogItem();
                                item.level = reader["level"].ToString();
                                item.X = reader["X"].ToString();
                                item.Y = reader["Y"].ToString();
                                item.facenum = int.Parse(reader["facenum"].ToString());
                                item.filename = reader["filename"].ToString();
                                item.found = reader["found"].ToString();
                                item.imgname = reader["imgname"].ToString();
                                item.detail = reader["detail"].ToString();
                                list.Add(item);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return list;
        }
        public string createTable()
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        var sb = new StringBuilder();
                        cmd.CommandText = "CREATE TABLE public." + table_xdo + "(" +
                            "\"Level\" integer," +
                            "\"X\" text," +
                            "\"Y\" text," +
                            "\"fileName\" text," +
                            "\"ObjectID\" integer," +
                            "\"Key\" text," +
                            "\"ObjBox_minX\" double precision," +
                            "\"ObjBox_minY\" double precision," +
                            "\"ObjBox_minZ\" double precision," +
                            "\"ObjBox_maxX\" double precision," +
                            "\"ObjBox_maxY\" double precision," +
                            "\"ObjBox_maxZ\" double precision," +
                            "\"Altitude\" real," +
                            "\"FaceNum\" integer," +
                            "\"XDOVersion\" integer," +
                            "\"VertexCount\" integer[]," +
                            "\"IndexedCount\" integer[]," +
                            "\"ImageLevel\" integer[]," +
                            "\"ImageName\" text[]" +
                            ")";
                        cmd.CommandText += "; CREATE TABLE public." + table_xdo_log + "(" +
                            "\"level\" text," +
                            "\"X\" text," +
                            "\"Y\" text," +
                            "\"filename\" text," +
                            "\"facenum\" integer," +
                            "\"imgname\" text," +
                            "\"found\" text," +
                            "\"detail\" text" +
                            ")";
                        cmd.CommandText += ";CREATE TABLE public." + table_dat + "(" +
                            "\"level\" integer," +
                            "\"IDX\" integer," +
                            "\"IDY\" integer," +
                            "\"filename\" text, " +
                            "\"ObjCount\" integer   ," + 
                            "\"Version\" integer[]," +
                            "\"Key\" text[]," +
                            "\"CenterPos_X\" double precision[] ," +
                            "\"CenterPos_Y\" double precision[]  ," +
                            "\"Altitude\" real[]," +
                            "\"ImageLevel\" integer[]," +
                            "\"dataFile\" text[], " +
                            "\"imgFileName\" text[], " +
                            "\"boxMinX\" double precision[]," +
                            "\"boxMinY\" double precision[]," +
                            "\"boxMinZ\" double precision[]," +
                            "\"boxMaxX\" double precision[]," +
                            "\"boxMaxY\" double precision[]," +
                            "\"boxMaxZ\" double precision[])";
                        cmd.CommandText += ";CREATE TABLE public." + table_dat_log + "(" +
                            "\"level\" text, " +
                            "\"X\" text, " +
                            "\"Y\" text, " +
                            "\"filename\" text, " +
                            "\"objCount\" text, " +
                            "\"xdoname\" text, " +
                            "\"found\" text, " +
                            "\"detail\" text" +
                            ")";
                        cmd.ExecuteNonQuery();
                        return table_dat + ", " + table_dat_log + ", " + table_xdo + ", " + table_xdo_log + "가 성공적으로 생성되었습니다.";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return "Table을 생성시키지 못했습니다.";
                }
            }
        }
        private NpgsqlConnection connection()
        {
            return new NpgsqlConnection("Host=" + info.Host + ";Port=" + info.Port + ";Username=" + info.Username + ";Password=" + info.Password + ";Database=" + info.Database);
        }
        public string clearTable()
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "delete from " + table_xdo + ";" +
                            "delete from " + table_xdo_log + ";" +
                            "delete from " + table_dat + ";" +
                            "delete from " + table_dat_log + ";";
                        cmd.ExecuteNonQuery();
                        return "Table을 성공적으로 초기화하였습니다.";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "초기화 실패";
                }
            }
        }
        public string deleteTable()
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "drop table " + table_xdo + "," + table_xdo_log + "," + table_dat + "," + table_dat_log;
                        cmd.ExecuteNonQuery();
                        return "Table을 성공적으로 삭제하였습니다";
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "삭제 실패";
                }
            }
        }
        public List<CheckVersionListItem> checkVersion(string path, int min, int max)
        {
            var list = new List<CheckVersionListItem>();
            var count = 0;
            var DATdirectorySet = new DirectoryFinder(path, min, max).run(EXT.DAT);

            foreach (string DATFolderPath in DATdirectorySet)
            {
                var DATFileList = new FileFinder(DATFolderPath).run(EXT.DAT);
                var hashMap = new Dictionary<string, ReadDAT>();

                foreach (string datFile in DATFileList)
                {
                    var dat = new ReadDAT(datFile);
                    var baseDirectory = new FileInfo(datFile).Directory.FullName;
                    
                    for(int i = 0; i < dat.body.Count; i++)
                    {
                        var xdoFileName = dat.body[i].dataFile;
                        var xdoURL = baseDirectory + @"\" + Path.GetFileNameWithoutExtension(datFile) + @"\" + xdoFileName;
                        var version = Int32.Parse((int)dat.body[i].version[0] + "" + (int)dat.body[i].version[1] + "" + (int)dat.body[i].version[2] + "" + (int)dat.body[i].version[3]);
                        if (File.Exists(xdoURL))
                        {
                            var xdo = new ReadXDO(xdoURL, version);
                            var logItem = new CheckVersionListItem();
                            logItem.level = new FileInfo(xdoURL).Directory.Parent.Parent.Name;
                            var y_x = new FileInfo(xdoURL).Directory.Name.Split('_');
                            logItem.y = y_x[0];
                            logItem.x = y_x[1];
                            logItem.datname = new FileInfo(datFile).Name;
                            logItem.DATversion = version.ToString();
                            logItem.xdoname = xdoFileName;
                            if (!xdo.isEnd && version == 3001)
                            {
                                logItem.XDOversion = 3002.ToString();
                                list.Add(logItem);
                                // repairXdoDictionary[LOG.XDO_VERSION_ERROR].Add(new RepairXDO(new ReadXDO(xdoURL), logItem.DATversion));
                            }
                            else if(!xdo.isEnd && version == 3002)
                            {
                                logItem.XDOversion = 3001.ToString();
                                list.Add(logItem);
                                // repairXdoDictionary[LOG.XDO_VERSION_ERROR].Add(new RepairXDO(new ReadXDO(xdoURL), logItem.DATversion));
                            }
                            else
                            {
                                // Console.WriteLine("request: " + xdoURL + "(" + version + ")");
                                // Console.WriteLine("response: " + xdo.isEnd);
                            }
                        }

                        count++;
                    }
                }
            }
            Console.WriteLine(list.Count + "/" + count);
            return list;
        } 
        public string connect()
        {
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    return "성공적으로 연결되었습니다.";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return "DB 접속 실패";
                }
            }
        }
        public void repair(Dictionary<LOG, HashSet<RepairXDO>> repairXdoDictionary, List<XDOLogItem> log, bool? isRepair)
        {
            foreach (KeyValuePair<LOG, HashSet<RepairXDO>> key in repairXdoDictionary)
            {
                foreach (var readXDO in key.Value)
                {
                    if(isRepair == true)
                    {
                        var xdo = readXDO.xdo;
                        switch (key.Key)
                        {
                            case LOG.XDO_VERSION_ERROR:
                                new WriteXDO(xdo, "backup", readXDO.reference);
                                break;
                            case LOG.WARN_CASE_SENSITIVE:
                                if (readXDO.index >= 0)
                                {
                                    var imgName = Path.Combine(new FileInfo(xdo.url).Directory.FullName, readXDO.reference.Replace(".", "_" + readXDO.index + "."));
                                    Console.WriteLine(imgName);
                                    if (File.Exists(imgName))
                                    {
                                        var real_img = Directory.GetFiles(Path.GetDirectoryName(Path.GetFullPath(imgName)), Path.GetFileName(Path.GetFullPath(imgName))).Single();
                                        if (imgName != real_img)
                                        {
                                            for (int i = 0; i < xdo.mesh.Count; i++)
                                            {
                                                if (xdo.mesh[i].imageName.ToLower().Equals(readXDO.reference.ToLower()))
                                                {
                                                    xdo.mesh[i].imageName = readXDO.reference.ToLower();
                                                    xdo.mesh[i].ImageNameLen = (byte)readXDO.reference.ToLower().Length;
                                                }
                                            }
                                            File.Move(real_img, new FileInfo(real_img).Directory.FullName + "temp.tmp");
                                            File.Move(new FileInfo(real_img).Directory.FullName + "temp.tmp", imgName.ToLower());
                                        }
                                    }
                                }
                                else
                                {
                                    var imgName = Path.Combine(new FileInfo(xdo.url).Directory.FullName, readXDO.reference);
                                    Console.WriteLine("basic\n" + imgName);
                                    if (File.Exists(imgName))
                                    {
                                        var real_img = Directory.GetFiles(Path.GetDirectoryName(Path.GetFullPath(imgName)), Path.GetFileName(Path.GetFullPath(imgName))).Single();
                                        if (imgName != real_img)
                                        {
                                            for (int i = 0; i < xdo.mesh.Count; i++)
                                            {
                                                if (xdo.mesh[i].imageName.ToLower().Equals(readXDO.reference.ToLower()))
                                                {
                                                    xdo.mesh[i].imageName = readXDO.reference.ToLower();
                                                    xdo.mesh[i].ImageNameLen = (byte)readXDO.reference.ToLower().Length;
                                                }
                                            }
                                            File.Move(real_img, new FileInfo(real_img).Directory.FullName + "temp.tmp");
                                            File.Move(new FileInfo(real_img).Directory.FullName + "temp.tmp", imgName.ToLower());
                                        }
                                    }
                                }
                                new WriteXDO(xdo, "", null);
                                break;
                        }
                    }
                }
            }
        }
        public void repair(Dictionary<LOG, HashSet<string>> repairDatDictionary, List<DATLogItem> log, bool? isRepair)
        {
            foreach (KeyValuePair<LOG, HashSet<string>> key in repairDatDictionary)
            {
                foreach (var URL in key.Value)
                {
                    if(File.Exists(URL) && isRepair == true)
                    {
                        var readDAT = new ReadDAT(URL);
                        var removeLater = new List<int>();
                        var dataFileList = new List<string>();
                        for (int i = 0; i < readDAT.body.Count; i++)
                            dataFileList.Add(readDAT.body[i].dataFile);
                        switch (key.Key)
                        {
                            case LOG.DUPLICATE_XDO:
                                var duplicates = dataFileList.Select((t, i) => new { Index = i, Text = t }).GroupBy(g => g.Text).Where(g => g.Count() > 1);
                                foreach (var group in duplicates)
                                {
                                    int count = 0;
                                    foreach (var x in group)
                                    {
                                        if (count++ == 0) continue;
                                        removeLater.Add(x.Index);
                                    }
                                }
                                foreach (var index in removeLater.OrderByDescending(x => x))
                                {
                                    readDAT.body.RemoveAt(index);
                                }
                                break;

                            case LOG.ERR_NOT_EXIST:
                                for (int i = 0; i < dataFileList.Count; i++)
                                {
                                    var xdo = Path.Combine(new FileInfo(readDAT.url).Directory.FullName, Path.GetFileNameWithoutExtension(readDAT.url), dataFileList[i]);
                                    if (!File.Exists(xdo))
                                    {
                                        removeLater.Add(i);
                                    }
                                }

                                foreach (var index in removeLater.OrderByDescending(x => x))
                                {
                                    readDAT.body.RemoveAt(index);
                                }
                                break;

                            case LOG.WARN_CASE_SENSITIVE:
                                for (int i = 0; i < dataFileList.Count; i++)
                                {
                                    var DAT_xdo = Path.Combine(new FileInfo(readDAT.url).Directory.FullName, Path.GetFileNameWithoutExtension(readDAT.url), dataFileList[i]);
                                    DAT_xdo = Path.GetFullPath(DAT_xdo);
                                    if (File.Exists(DAT_xdo))
                                    {
                                        var REAL_xdo = Directory.GetFiles(Path.GetDirectoryName(Path.GetFullPath(DAT_xdo)), Path.GetFileName(Path.GetFullPath(DAT_xdo))).Single();
                                        if (DAT_xdo != REAL_xdo)
                                        {
                                            var realXDOName = new FileInfo(REAL_xdo).Name;
                                            readDAT.body[i].dataFile = realXDOName;
                                            readDAT.body[i].dataFileLen = (byte)realXDOName.Length;
                                        }
                                    }

                                }
                                break;
                        }
                        new WriteDAT(readDAT, "backup");
                    }
                    else
                    {
                        var fileInfo = new FileInfo(URL);
                        var yx = fileInfo.Name.Split('_');
                        switch (key.Key)
                        {
                            case LOG.DAT_CANNOT_PARSE_INVALID_XDONAME:
                                log.Add(new DATLogItem(fileInfo.Directory.Parent.Name, yx[0], yx[1], key.Key, "", "", "", ""));
                                break;
                            case LOG.DAT_CANNOT_PARSE_NOT_EXIST_DIRECTORY:
                                log.Add(new DATLogItem(fileInfo.Directory.Parent.Name, yx[0], yx[1], key.Key, "", "", "", ""));
                                break;
                        }
                    }
                }
            }
        }

        public void makeGLTF(string path, int min, int max, BackgroundWorker worker)
        {
            const string saveFolderName = "gltf";

            var Watch = new Stopwatch();
            Watch.Restart();


            var DATdirectorySet = new DirectoryFinder(path, min, max).run(EXT.DAT);
            var saveBasePath = new FileInfo(path).Directory.FullName;
            worker.ReportProgress(0, new ReportProgressItemClass(DATdirectorySet.Count));
            
            foreach (string DATFolderPath in DATdirectorySet)
            {
                var DATFileList = new FileFinder(DATFolderPath).run(EXT.DAT);

                foreach(string datFile in DATFileList)
                {
                    var dat = new ReadDAT(datFile);
                    var Y = Path.GetFileNameWithoutExtension(datFile).Split('_');
                    var savePath = Path.Combine(saveBasePath, saveFolderName, dat.header.level.ToString(), Y[0], Path.GetFileNameWithoutExtension(datFile));

                    var di = Directory.CreateDirectory(savePath);

                    for(int i = 0; i < dat.body.Count; i++)
                    {
                        var xdoPath = Path.Combine(new FileInfo(datFile).Directory.FullName, Path.GetFileNameWithoutExtension(datFile), dat.body[i].dataFile);
                        if (File.Exists(xdoPath))
                        {
                            
                            var version = dat.body[i].version;
                            var versionStr = Int32.Parse(string.Format("{0}{1}{2}{3}", version[0], version[1], version[2], version[3]));
                            try
                            {
                                var xdo = new ReadXDO(xdoPath, versionStr);
                                new GLTF(xdo, savePath);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        }
                    }
                }

                worker.ReportProgress(1);

                //var XDOFileList = new FileFinder(XDOPath).run(EXT.XDO);

                //foreach(var xdofile in XDOFileList)
                //{
                //    var filename = new FileInfo(xdofile).Name;
                //    var directory = new FileInfo(xdofile).Directory.FullName;
                //    new GLTF(new ReadXDO(xdofile), filename, directory);
                //    Console.WriteLine(Path.Combine(directory, filename));
                //}
            }
        }

        public string getInvalidFileList(string path)
        {
            var sb = new StringBuilder();
            sb.Append("");
            using (var conn = connection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "select * from " + table_xdo_log + " where detail='" + "Invalid filename in folder(carriage return or line feed)" +"';";

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var temp_sb = new StringBuilder();
                                temp_sb.Append(path);
                                temp_sb.Append(@"\");
                                temp_sb.Append(reader["level"].ToString());
                                temp_sb.Append(@"\");
                                temp_sb.Append(reader["Y"].ToString());
                                temp_sb.Append(@"\");
                                temp_sb.Append(reader["Y"].ToString());
                                temp_sb.Append(@"_");
                                temp_sb.Append(reader["X"].ToString());

                                sb.AppendLine(temp_sb.ToString());

                                temp_sb = null;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return sb.ToString();
        }
    }
    
}
