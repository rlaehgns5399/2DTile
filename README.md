# 2DTile

When the shape is given, I want to know which tile this shape belongs to

# XDOErrorDectectorUI

![Something](https://user-images.githubusercontent.com/26527826/48317753-20591200-e63a-11e8-891f-295913eb85b9.png)

Look how simple this program is.

Top label means directory which you inputed.

If you want to change directory, go to `postgreSQL.cs`, line 14

```
public String baseURL = @"C:\Users\KimDoHoon\Desktop\C++_Project\data";
```

and you must change postgreSQL information.

In `postgreSQL.cs`, line 157~164

```
    class DB
    {
        public String Host = "localhost";
        public String Username = "postgres";
        public String Password = "root";
        public String Database = "mydata";
        public String Table = "xdo";
    }
```

and structure of table(xdo) is like this:

![image](https://user-images.githubusercontent.com/26527826/48317831-5d71d400-e63b-11e8-92c2-858debe25d8a.png)

**Update** button does:

- find `.xdo` file in given directory
- parsing `.xdo`, check if there is texture
```
success: there is correct texture.
error: there is no correct texture(missing file)
warning: there is texture, but something problem(Upper, LowerCase)
```
- delete all data in table(`xdo`)
- insert data in table(`xdo`)


**Check** button does:

![image](https://user-images.githubusercontent.com/26527826/48317770-6f06ac00-e63a-11e8-86d5-e9791191b4ba.png)

- read DB & gets its rows.
- change label of bottom to row size.
- fill listview


