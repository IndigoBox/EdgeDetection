# Lines - Open Source Edge Detection

Lines is a simple edge detection software which analyzes an image and detects edges, which it displays as black pixels in the second image slot. Lines can also run a separate algorithm on the image after it has been analyed, which attempts to split the image into separate objects, or at least separate groups of edges. Lines is completely open source, and all code for it is right here on GitHub! To find out more on how you may use and distribute Lines, please look at the licensing notes.

### Current Features

- Analyze images for edges, detecting contrast in the image
- Separate groups of edge pixels in the images, and color them

### Planned Features

 - Add Webcam Support
 - Improve efficiency
 - Create threshold detection (a dark image with mostly dark
 colors will be analyzed in higher detail then one that is
 well lit with lots of contrast)
 - Improve the separate algorithm so that each edge group is
 an object, and is seperated from overlapping objects, and 
 grouped with other edges of the same object

### Licensing Notes

-   This software was created by **Viktor Koves**
-   This software is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
-   For more information on licensing, click [here][1], or contact me.

[1]:http://creativecommons.org/licenses/by-nc-sa/3.0/deed.en_US
