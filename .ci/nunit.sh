#!/bin/sh -x

mono --runtime=v4.0 .nuget/NuGet.exe install NUnit.Runners -Version 2.6.3 -o packages

runTest(){
   mono --runtime=v4.0 packages/NUnit.Runners.2.6.3/tools/nunit-console.exe -noxml -nodots -labels -stoponerror $@
   if [ $? -ne 0 ]
   then
     exit 1
   fi
}

#This is the call that runs the tests and adds tweakable arguments.
runTest $1

exit $?
