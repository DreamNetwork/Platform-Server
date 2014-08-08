#!/bin/sh
ORIGINAL_MONO_VER="${MONO_VER}"
MONO_VER_PREFIX=""
if [ "${MONO_VER}" == "2.10.8_3" ]; then
	MONO_VER="2.10.8"
	MONO_VER_PREFIX="3/"
fi

wget -q -Omono.pkg "http://download.xamarin.com/MonoFrameworkMDK/Macx86/MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.pkg" || \
	wget -q -Omono.pkg "http://download.xamarin.com/MonoFrameworkMDK/Macx86/${MONO_VER_PREFIX}MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.pkg" || \
	wget -q -Omono.pkg "http://download.mono-project.com/archive/${MONO_VER}/macos-10-x86/MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.pkg" || \
	wget -q -Omono.pkg "http://download.mono-project.com/archive/${MONO_VER}/macos-10-x86/${MONO_VER_PREFIX}MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.pkg" || \
	wget -q -Omono.dmg "http://download.xamarin.com/MonoFrameworkMDK/Macx86/MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.dmg" || \
	wget -q -Omono.dmg "http://download.xamarin.com/MonoFrameworkMDK/Macx86/${MONO_VER_PREFIX}MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.dmg" || \
	wget -q -Omono.dmg "http://download.mono-project.com/archive/${MONO_VER}/macos-10-x86/MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.dmg" || \
	wget -q -Omono.dmg "http://download.mono-project.com/archive/${MONO_VER}/macos-10-x86/${MONO_VER_PREFIX}MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.dmg" || exit -1

if [ -e "mono.dmg" ]; then
	hdid "mono.dmg"
	sudo installer -pkg "/Volumes/Mono Framework MDK ${MONO_VER}/MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.pkg" -target / || \
		sudo installer -pkg "/Volumes/MonoFramework-MDK-${MONO_VER}/MonoFramework-MDK-${ORIGINAL_MONO_VER}.macos10.xamarin.x86.pkg" -target /
else
	sudo installer -pkg "mono.pkg" -target /
fi
