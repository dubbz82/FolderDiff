# FolderDiff

This application is used for creating and restoring diff's for 2 different folder's contents.  Under the hood it uses bsDiff for generating patches.  Currently there are some limitations and some future enhancements
 - Limitations:
   - This currently expects the files in both the parent and any other folders to be processed in the same order.  This will process through multiple folders and create patches for several at once if available.
   - RAM usage - currently this uses roughly 10x the file's size in ram, although it currently processes only one file at a time, I have intention to potentially multithread this at some point...keep this in mind if you're processing large files, as a 2gb source file will potentially use upwards of 20gb of ram while processing
   - Long processing times - Depending on the exact input, this can sometimes take a considerable amount of time to run through a folder. Sometimes the process will seem to get "stuck" on a file, and eventually process...other times it will ACTUALLY be stuck.  In current state I wouldn't consider this "production ready"
  
- Bugs
  - The above mentioned issue where files sometimes spin endlessly and never finish processing.  This is a problem with the library that's being used and I would like to look into this and address it in the future.
  - Additional issue with the library where the file doesn't properly clear from RAM.  This is mitigated for now with a direct call to the garbage collector, but there's some performance overhead doing this and it may be problematic when processing a large number of smaller files
- Future Ideas for making this better?
  - Custom mapping of files in source and target folders to allow for mapping where the source and destination files aren't in the same order..
  - Multithreading
  - Support for XDelta
  - One-off file patching instead of just bulk mode
  - Replace the BSDiff library with something a bit better.  There's some experimental stuff in the codebase right now, but as of time of writing, none of this is better than what's there, and may not even fully work correctly.. 
