# �V�F�[�_�[�R���p�C���p�X�N���v�g
# �r���h�Ŏ��s����Ȃ��̂Ŏ蓮�Ŏ��s����

$fxc ="C:\Program Files (x86)\Windows Kits\10\bin\x86\fxc.exe"

trap {break}

foreach($fx in Get-ChildItem "*.fx")
{
    $ps = [System.IO.Path]::ChangeExtension($fx, ".ps");
    & $fxc $fx /T ps_2_0 /Fo $ps
    if(!$?)
    {
        throw "compile error."
    }
}
