# League of Legends MiniMap Viewport Finder

This command line application finds the location of the white viewport rectangle in the MiniMap of League of Legends screenshots.

## Usage

Run `LOLViewportFinder.exe` to process all files from `inputFiles.txt`. `inputFiles.txt` requires an URL or local file path per line.

The results are printed to stdout.

Images from URLs are cache in the folder `.\inputCache` for subsequent runs.

### Debug Output

Add the `-d` argument have the application write intermediate images to the folder `.\output`.