[English](README.md) | [日本語](README.ja.md) | **Français**

# Exemple d'intégration d'axr_runtime — macOS (Intel / x64)

Aucune compilation n'est nécessaire. `native/libaxr_runtime.dylib` est
inclus.

## 1. Python (ctypes)

Aucune dépendance supplémentaire n'est requise (uniquement `ctypes` de
la bibliothèque standard).

```bash
cd python
python3 render_sample.py
```

## 2. C# (.NET, P/Invoke)

Le SDK .NET 10 est requis. Vous pouvez l'installer avec
`brew install --cask dotnet-sdk`.

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

Avec Homebrew, `brew install openjdk maven` installe le JDK et Maven
en une seule commande.

Les 4 langages ont été vérifiés sur une machine réelle (macOS, Intel
x64).

## Licence et activation

Sans activation, le PDF généré comporte un filigrane. Si vous disposez
d'une clé de licence, activez avant d'exécuter. Vérifiez la présence
du filigrane en ouvrant `output.pdf`.

```bash
ACR_LICENSE_EMAIL=you@example.com ACR_LICENSE_KEY=ACR-xxxx-xxxx-xxxxxxxx-xxxxxxxx \
    python3 render_sample.py
```
