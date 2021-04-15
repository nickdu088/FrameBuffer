# FrameBuffer

Since there is not much info about .Net Core and Linux FrameBuffer.
I created this small sample for .Net Core and [Linux FrameBuffer](https://en.wikipedia.org/wiki/Linux_framebuffer).
It shows how to display a bmp picture (sample.bmp) on framebuffer. Theoretically you should be able to create your own graphic render engine without any 3rd party library.

Usage:

```
dotnet FrameBuffer.dll /dev/fb0
```
