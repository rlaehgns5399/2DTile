﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XDOErrorDetectorUI
{
    public class RepairXDO
    {
        public ReadXDO xdo;
        public string reference;
        public int index = -1;
        public RepairXDO(ReadXDO xdo, string reference)
        {
            this.xdo = xdo;
            this.reference = reference;
        }
        public RepairXDO(ReadXDO xdo, string reference, int index) : this(xdo, reference)
        {
            this.index = index;
        }
    }

    public class RepairDAT
    {
        public ReadDAT dat;
        public string reference;
        public RepairDAT(ReadDAT dat, string reference)
        {
            this.dat = dat;
            this.reference = reference;
        }
        
    }
}
