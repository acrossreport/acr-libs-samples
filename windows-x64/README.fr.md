[English](README.md) | [日本語](README.ja.md) | **Français**

# Exemple d'intégration d'axr_runtime — Windows (x64)

Aucune compilation n'est nécessaire. `native/axr_runtime.dll` est
inclus.

## 1. Python (ctypes)

Aucune dépendance supplémentaire n'est requise (uniquement `ctypes` de
la bibliothèque standard).

```bash
cd python
python render_sample.py
```

## 2. C# (.NET, P/Invoke)

Le SDK .NET 10 est requis.

```bash
cd csharp
dotnet run
```

## 3. VB.NET (.NET, P/Invoke)

Le SDK .NET 10 est requis.

```bash
cd vb
dotnet run
```

## 4. Java (JNA)

JDK 11+ et Maven sont requis. `pom.xml` récupère `net.java.dev.jna:jna`
depuis Maven Central (connexion réseau requise).

```bash
cd java
mvn package
java -jar target/acr-libs-sample-jar-with-dependencies.jar
```

Python / C# / VB.NET ont été vérifiés sur une machine réelle. Le code
Java est identique à celui des autres plateformes, mais il n'a pas été
exécuté ici car Maven n'est pas installé sur cette machine.

## Licence et activation

Sans activation, le PDF généré comporte un filigrane. Si vous disposez
d'une clé de licence, activez avant d'exécuter. Vérifiez la présence
du filigrane en ouvrant `output.pdf`.

```bash
# avec cmd.exe
set ACR_LICENSE_EMAIL=you@example.com
set ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx
python render_sample.py
```

```powershell
# avec PowerShell
$env:ACR_LICENSE_EMAIL = "you@example.com"
$env:ACR_LICENSE_KEY = "ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx"
python render_sample.py
```
