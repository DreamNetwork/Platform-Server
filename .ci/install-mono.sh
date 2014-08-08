#!/bin/sh -eux
MONO_VER_PREFIX=""
if [ "${MONO_VER}" == "2.10.8_3" ]; then
	MONO_VER_PREFIX="3/"
fi

wget -Omono.pkg "http://download.xamarin.com/MonoFrameworkMDK/Macx86/MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.pkg" || \
	wget -Omono.pkg "http://download.xamarin.com/MonoFrameworkMDK/Macx86/${MONO_VER_PREFIX}MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.pkg" || \
	wget -Omono.pkg "http://download.mono-project.com/archive/${MONO_VER}/macos-10-x86/${MONO_VER_PREFIX}MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.pkg" || \
	wget -Omono.pkg "http://download.mono-project.com/archive/${MONO_VER}/${MONO_VER_PREFIX}macos-10-x86/${MONO_VER_PREFIX}MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.pkg" || \
	wget -Omono.dmg "http://download.xamarin.com/MonoFrameworkMDK/Macx86/MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.dmg" || \
	wget -Omono.dmg "http://download.xamarin.com/MonoFrameworkMDK/Macx86/${MONO_VER_PREFIX}MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.dmg" || \
	wget -Omono.dmg "http://download.mono-project.com/archive/${MONO_VER}/macos-10-x86/MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.dmg" || \
	wget -Omono.dmg "http://download.mono-project.com/archive/${MONO_VER}/${MONO_VER_PREFIX}macos-10-x86/MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.dmg" || \

if [ -e "mono.dmg" ]; then
	hdid "MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.dmg"
	sudo installer -pkg "/Volumes/Mono Framework MDK ${MONO_VER}/MonoFramework-MDK-${MONO_VER}.macos10.xamarin.x86.pkg" -target /
else
	sudo installer -pkg "mono.pkg" -target /
fi
