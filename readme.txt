# Custom MSBuild Logger for Building Blender on Windows (with MSVC 2013)

This implements a custom logger component to be supplied to MSBuild when compiling Blender.
It is designed to specifically ignore all the "subprojects" (i.e. everything ending in .lib)
that MSBuild reports as being ready, because most of the time nothing has changed. We do this
because, with all these un-useful reports, it's hard to see the ones we actually do care about
(i.e. the warnings/errors about different files, and also which files it is compiling). Currently,
it is hard to tell if a recompile actually did anything to recompile the file we changed or not
(i.e. we may have forgotten to save the file, and then wonder why our fix didn't work).


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
**Tutorials:**
 * https://helloacm.com/c-custom-logger-sample-for-msbuild/


**Sample Loggers:**
 * https://github.com/mikefourie/MSBuildExtensionPack


**Reference Docs:**
 * Build Loggers Ref -- https://msdn.microsoft.com/en-us/library/ms171471(v=vs.120)  
 * Logging in Multi-Processor Environments -- https://msdn.microsoft.com/en-us/library/bb383987.aspx

