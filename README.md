# 2DTile

When the shape is given, I want to know which tile this shape belongs to

## Problem description

![image](https://user-images.githubusercontent.com/26527826/48421740-6e882580-e7a0-11e8-816b-f76e8e1f7cbd.png)

Let's say zero from the bottom left.

<pre>
2(0,1)	3(1,1)
0(0,0)	1(1,0)
</pre>
[level 0 with shapes(set of points)]

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

# QuadtreeUI

![image](https://user-images.githubusercontent.com/26527826/48673764-30b44400-eb88-11e8-8f62-f33f422e71a8.png)

Imperfect quadtree & shape (ver1)

![image](https://user-images.githubusercontent.com/26527826/48693180-fbf1cc80-ec1b-11e8-9f93-ee299d3e8a7e.gif)

Nice quadtree but there is something to be fixed

![image](https://user-images.githubusercontent.com/26527826/48844184-76694a80-eddc-11e8-82f7-f1ae6cf0fd3f.png)

Because there is an error when a line passes tile which is judged correct already

So, with [an Article](http://bowbowbow.tistory.com/17), i fixed this error. Previosly, The pink tile was be perceived as correct. But not now(2018. 11. 21)

![honeycam 2018-11-23 20-41-37](https://user-images.githubusercontent.com/26527826/48941807-5cf10b80-ef60-11e8-9019-d8b7c5bdbf1f.gif)

I completed this work. (I think), (2018. 11. 23)

As you see, Each tile is represented by a different color with each level.

The request wanted to be represented to the level 15. 

In my computer, beyond level 11, my computer slows down in representing shape.

I made Quadtree, but it doesn't need(of course, there were good ideas to refer).

For making shape, I havent use `Convex hull algorithm`.

If you want to use, just modify `Point` class codes.

<hr>

# XDOErrorDectectorUI

This program's purposes are showing DAT, XDO's inner data & checking its referred texture files.

You can input information for connecting DB(using `PostgreSQL`). if the connection is successful, you can set table where you access.

you can create, delete, clear table you inputted.

`테이블 불러오기(Load Table)` does:

- load data into listview

`검색 및 DB에 저장(DAT, XDO Search & Save at DB)` does:

- The program will find recursively DAT, XDO files in given folder(beware about out of memory)
- The program will parse DAT, XDO files, check texture error, and make a note unused textures.
- After above that, it will save data at DB
