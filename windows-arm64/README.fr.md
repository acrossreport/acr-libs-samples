[English](README.md) | [日本語](README.ja.md) | **Français**

# Exemple d'intégration d'axr_runtime — Windows (ARM64)

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

À propos du JDK sous Windows ARM64 : Eclipse Temurin ne propose pas de
build Windows ARM64 pour la branche JDK 17, utilisez donc **JDK 21 ou
une version plus récente**
([build aarch64_windows de Temurin 21](https://github.com/adoptium/temurin21-binaries/releases)).
Les installations via la plupart des gestionnaires de paquets ne
proposent souvent qu'une version x64, qui tourne alors en émulation sur
Windows ARM64 et échoue à charger `axr_runtime.dll` (natif ARM64).
Vérifiez que `os.arch` affiché par
`java -XshowSettings:properties -version` est bien `aarch64`.

Les 4 langages ont été vérifiés sur une machine réelle.
