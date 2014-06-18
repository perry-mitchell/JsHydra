JsHydra
=======

A simple JavaScript code coverage tool written in C# for Mono.

About
-----

Hydra is a tool I wrote to handle simple JavaScript code coverage measurements by adding hooks to the code. The code is processed by the application, and hosted in a web server for serving to test suites and the like. The injected hooks measure code usage and POST the results back to the Hydra server.

Why use Hydra?
--------------

Hydra requires no modification to the original source - so no updates to your library are necessary. Just load the script with Hydra and use the Hydra source URL in your webpage. Using your application will then record usage statistics within Hydra, collecting coverage data.

Installing the Mono library on your operating system is all that's required to run Hydra, as no OS-dependent libraries are used.

Building
--------

To build the application, simply run the bash script:

	./build

This will create a folder labelled "output", which will contain the executable (hydra.o) along with some resources.

Usage
-----

If you have a script at "http://example.com/library.js", you would run it through Hydra like so:

	./hydra.o http://example.com/library.js

Depending on the size of your library, it may take some time to prepare the code within the application. To use the coverage measurements, replace the reference to your script file with the Hydra URL - for example:

	http://localhost:9999/hydra/source/

