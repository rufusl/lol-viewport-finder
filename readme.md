# League of Legends MiniMap Viewport Finder

This command line application finds the location of the white viewport rectangle in the MiniMap of League of Legends screenshots.

## Usage

Run `LOLViewportFinder.exe` to process all files from `inputFiles.txt`. `inputFiles.txt` requires an URL or local file path per line.

The results are printed to stdout.

Images from URLs are cache in the folder `.\inputCache` for subsequent runs.

### Debug Output

Add the `-d` argument have the application write intermediate images to the folder `.\output`.


## Open Improvements

* improve image processing performance
  * could possibly be done on the GPU
  * faster image cropping
  * better image handling library than System.Drawing?
* there is probably also a faster blob detection algorithm
* image is read twice into buffer array, can probably be done with one buffer only
* rectangle detection will currently also detect filled rectangles, this could be improved by detecting lines only up to a certain thickness (for now, line thickness is ignored)
 