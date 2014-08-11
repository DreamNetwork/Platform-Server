#!/bin/sh

mono --runtime=v4.0 .nuget/NuGet.exe install NUnit.Runners -o packages

runTest(){
	# check mono version to avoid specific bugs
	monoVersion=`mono --version | head -n1 | cut -f1 -d"(" | sed 's/[[:alpha:]|(|[:space:]]//g' | awk -F- '{print $1}'`
	monoArgs="--runtime=v4.0"
	case "$monoVersion" in
		2.*)
		3.0.*)
			echo "Disabling gshared optimization to circumvent mono bugs."
			monoArgs="$monoArgs -O=-gshared"
		;;
	esac

   mono $monoArgs packages/NUnit.Runners.*/tools/nunit-console.exe -noxml -nodots -labels $@
   if [ $? -ne 0 ]
   then
     exit 1
   fi
}

#This is the call that runs the tests and adds tweakable arguments.
runTest $1

exit $?
