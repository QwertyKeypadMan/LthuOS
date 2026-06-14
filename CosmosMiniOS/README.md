# Cosmos Mini OS

Bu klasor, Cosmos Kernel kullanarak hazirlanmis basit bir C# isletim sistemi iskeletidir.

## Ozellikler

- Cosmos.System.Kernel tabanli acilis
- Metin tabanli komut satiri
- `help`, `about`, `clear`, `echo`, `calc`, `time`, `reboot`, `shutdown` komutlari

## Gerekenler

- Visual Studio 2022
- Cosmos DevKit veya UserKit
- QEMU, VMware ya da Cosmos'un destekledigi bir sanal makine

Cosmos dokumanlarina gore Windows tarafinda DevKit kurulumu icin Cosmos kaynak kodundaki `install-VS2022.bat` kullanilir. Linux tarafinda ise Cosmos kaynak klasorunde `make` ile kurulum yapilabilir.

## Calistirma

1. `CosmosMiniOS.sln` dosyasini Visual Studio 2022 ile ac.
2. Cosmos eklentisi yukluyse projeyi derle.
3. Cosmos'un olusturdugu ISO'yu sanal makinede baslat.

Komut satiri acildiginda:

```text
help
about
echo merhaba
calc 10 + 5
clear
```

komutlarini deneyebilirsin.

## Not

Bu proje gercek donanim suruculeri veya dosya sistemi iceren buyuk bir OS degil; Cosmos uzerinde buyutulebilecek temiz bir baslangic kernelidir.
