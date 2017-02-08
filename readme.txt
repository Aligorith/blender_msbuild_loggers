# Custom MSBuild Logger for Building Blender on Windows (with MSVC 2013)

This implements a custom logger component to be supplied to MSBuild when compiling Blender.
It is designed to specifically ignore all the "subprojects" (i.e. everything ending in .lib)
that MSBuild reports as being ready, because most of the time nothing has changed. We do this
because, with all these un-useful reports, it's hard to see the ones we actually do care about
(i.e. the warnings/errors about different files, and also which files it is compiling). Currently,
it is hard to tell if a recompile actually did anything to recompile the file we changed or not
(i.e. we may have forgotten to save the file, and then wonder why our fix didn't work).


Features
--------

Short List:
* Simpler, Cleaner, and More Easily Scannable Output  
* Stop on First Error 

Long List:
1) It does not report those infernal ".vcxproj -> .lib" prints that are good for nothing
2) For each file that it compiles, it now says 'Compiling => "filename.c"', just like SCons used to (I've always liked this thing about my old buildsystem)
3) As for modules being relinked/updated, it now prints a 'Linking Lib => "bf_libname"' line, similar to what SCons used to do
4) More widespread use of colour. The colour coding for warnings (yellow) and errors (red) that the default logger had are retained (and improved), and other colour indicators are used for other key events.
5) Simpler formatting for error messages that prioritises showing you what you really need to know
6) Builds immediately terminate when the first error is encountered, instead of continuing to build the rest of the code. Unlike MSBuild's "StopOnFirstFailure" option, this *does* still work on multiprocessor builds.
7) It does not report all the "Cmake does not need to run" cruft at the start. Only when stuff has changed should it need to print anything there.
8) It reports number of warnings/errors in the code, so you have an idea of how much stuff to look out for.
9) Warnings about "zero-sized arrays" in structs/unions/anywhere else are silenced.


Usage
-----

1) Run the following command to perform the two steps involved in building/installing the logger:
   $ build && deploy

   For reference, the two steps performed here are:
   1a) Compile using the provided "build.bat"
   1b) Copy the CustomLogger.dll generated, and dump it in the root directory of the Blender sources

2) Make your own copy of make.bat, and modify the msbuild commandline to include
   /logger:CustomLogger.dll /noconsolelogger

   (XXX: Perhaps we don't need the noconsolelogger? We still want the warnings/errors...)
   
3) Run your modified make.bat



Useful Links
------------
Blog post introducing this tool:
* http://aligorith.blogspot.com/2017/02/custom-msbuild-loggers-for-blender-devs.html

Tutorials:
 * https://helloacm.com/c-custom-logger-sample-for-msbuild/


Sample Loggers:
 * https://github.com/mikefourie/MSBuildExtensionPack


Reference Docs:
 * Build Loggers Ref -- https://msdn.microsoft.com/en-us/library/ms171471(v=vs.120)  
 * Logging in Multi-Processor Environments -- https://msdn.microsoft.com/en-us/library/bb383987.aspx

