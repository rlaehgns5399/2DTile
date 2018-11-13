# 2DTile

When the shape is given, I want to know which tile this shape belongs to

## Problem description

`level 0` has 50 tiles(5 x 10), and level is up to **15**.

As the  level increases, each tile is divided into **`4`** smaller tiles. (i.e, lv1 has 5x10x4 tiles, lv2 has 5x10x4^2)

When the point set is given, it will compose a shape.

When `level 0`, the shape can cover a tile perfectly, I will write somewhere with correct format.

if the shape doesnt cover tile perfectly, it can be divide into 4 tiles(increasing **1** level). 

and compare until shape covers perfectly a tile.

return the sets of tile's x,y(like `level_n_x_y`)

### how to check how many tile cover

I will use straight equation and points of intersection(limited scope) 

## Reference

- [flood fill](https://ko.wikipedia.org/wiki/%ED%94%8C%EB%9F%AC%EB%93%9C_%ED%95%84) (i think this is not good for this problem)
- [QuadTree](https://en.wikipedia.org/wiki/Quadtree)


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


