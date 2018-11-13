# 2DTile

When the shape is given, I want to know which tile this shape belongs to

## Problem description

![image](https://user-images.githubusercontent.com/26527826/48421740-6e882580-e7a0-11e8-816b-f76e8e1f7cbd.png)

Let's say zero from the bottom left.

<pre>
2(0,1)	3(1,1)
0(0,0)	1(1,0)
</pre>
[level 0 with shapes(points of set]

![image](https://user-images.githubusercontent.com/26527826/48423325-bc525d00-e7a3-11e8-8c60-b0ac9d5390d9.png)

In `level 0`, because the shape cover tile 1, it returns **`lv0-1-0`(tile 1)**. (lv0, x:0, y:1)

but the remains exist, divide 0, 3 tiles into 4 tiles, and increase level 1

![image](https://user-images.githubusercontent.com/26527826/48423332-c07e7a80-e7a3-11e8-82dc-e63ede80af63.png)

Now, we have 8 tiles. Let's mark out with x, y coordinates, number.

<pre>
22(0,1.5)	23(0.5,1.5)	32(1,1.5)	33(1.5,1.5)
20(0,1)		21(0.5,1)	30(1,1)		31(1.5,1)
02(0,0.5)	03(0.5,0.5)	12(1,0.5)	13(1.5,0.5)
00(0,0)		01(0.5,0)	10(1,0) 	11(1.5,0)
</pre>

The shape 01(0.5,0) & 31(1.5,1) cover its tile. so it returns **`lv1-0.5-0`, `lv1-1.5-1`**.

but the remain exists. Let's divide 30(1,1) tile into 4 tiles

![image](https://user-images.githubusercontent.com/26527826/48423341-c4aa9800-e7a3-11e8-97ce-694cb7004491.png)

the 30(1,1) will be like this:

<pre>
302(1,1.25)	303(1.25,1.25)
300(1,1)	301(1.25,1)
</pre>

the tile 301(1.25,1) is covered. so it returns **`lv2-1.25-1`**.

Finally, we find all tiles! so exit this algorithm.

<hr>

`level 0` has 50 tiles(5 x 10), and level is up to **15**.

As the  level increases, each tile is divided into **`4`** smaller tiles. (i.e, lv1 has 5x10x4 tiles, lv2 has 5x10x4^2)

When the point set is given, it will compose a shape.

When `level 0`, the shape can cover a tile perfectly, I will write somewhere with correct format.

if the shape doesnt cover tile perfectly, it can be divide into 4 tiles(increasing **1** level). 

and compare until shape covers perfectly a tile.

return the sets of tile's x,y(like `level_n_x_y`)

### How to determine how many tiles the shape covers

I will use straight equation and points of intersection(limited scope) 

## Reference

- [flood fill](https://ko.wikipedia.org/wiki/%ED%94%8C%EB%9F%AC%EB%93%9C_%ED%95%84) (i think this is not good for this problem)
- [QuadTree](https://en.wikipedia.org/wiki/Quadtree)
- [Ray casting algorithm(Detect point in polygon)](https://en.wikipedia.org/wiki/Point_in_polygon)
- [Convex hull](https://en.wikipedia.org/wiki/Convex_hull_algorithms)

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


