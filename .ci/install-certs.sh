#!/bin/sh
mozroots --import --sync >/dev/null
yes yes | certmgr -ssl https://go.microsoft.com >/dev/null
yes yes | certmgr -ssl https://nugetgallery.blob.core.windows.net >/dev/null
yes yes | certmgr -ssl https://nuget.org >/dev/null