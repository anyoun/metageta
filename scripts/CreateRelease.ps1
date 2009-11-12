$ErrorActionPreference = "Stop"

function create-directory($dir) {
	if(-not (test-path $dir)) {
		(mkdir $dir).FullName + '\' #'
	} else {
		join-path (resolve-path $dir) '\' #'
	}
}


function recreate-directory($dir) {
	if(test-path $dir) {
		rmdir -Recurse $dir
	}
	create-directory $dir
}

if(test-path "hkcu:\Software\7-Zip") {
	$7zipPath = join-path (gi hkcu:\Software\7-Zip).GetValue("Path") "7z.exe"
} else {
	throw "No 7zip!"
}

if(test-path "$($env:windir)\Microsoft.NET\Framework64\v3.5") {
	$msbuildPath = "$($env:windir)\Microsoft.NET\Framework64\v3.5\msbuild.exe"
} else {
	throw "No msbuild!"
}

$timestamp = (get-date).ToString("yyyyMMdd")

pushd ..

[void](create-directory "output")
$dir = recreate-directory "output\MetaGeta.$timestamp"

& $msbuildPath MetaGeta\MetaGeta.sln /v:q /t:Rebuild "/p:Configuration=Release" "/p:Platform=Any CPU" "/p:OutDir=$dir;OutputPath=$dir"

pushd $dir
	rm *.xml
	rm *.pdb
	copy ..\..\tools\AtomicParsley\AtomicParsley.exe .
	copy ..\..\tools\ffmpeg\ffmpeg.exe .
popd

& $7zipPath a -mx9 -mmt "output\MetaGeta.$timestamp.7z" "$dir"

popd