[English](README.md) | [日本語](README.ja.md) | **Français**

# Exemples d'intégration

`axr_runtime` est une bibliothèque partagée native à ABI C, conçue
pour le designer Avalonia. Alors que `@acrossreport/acr-node` (un
addon Node.js réalisé avec napi-rs) est un exemple qui intègre ACR
depuis Node.js, ce dépôt contient des exemples minimaux qui **appellent
directement la même interface C FFI depuis quatre langages :
C# / VB.NET / Python / Java.**

**Aucune compilation n'est nécessaire.** Pour chaque plateforme
vérifiée, la bibliothèque et l'ensemble des exemples sont fournis sous
forme de distribution autonome. **Utilisez uniquement le dossier
correspondant à votre système et à votre processeur.**

Pour C / C++ (et pour cgo, lorsque vous appelez la bibliothèque depuis
Go), utilisez [`acr.h`](acr.h) à la racine de ce dépôt — il déclare la
même interface C que ces exemples appellent, et c'est l'en-tête à
inclure (`#include`/`cgo`).

```
acr-libs-samples/
├── acr.h            # En-tête ABI C (C/C++, cgo, ...)
├── windows-x64/     # Windows (x64)
│   ├── native/axr_runtime.dll
│   ├── native/axr_runtime.lib  # bibliothèque d'import, pour lier depuis C/C++ MSVC ou cgo
│   ├── common/      # JSON de template / JSON de données
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
├── windows-arm64/   # Windows (ARM64)
│   ├── native/axr_runtime.dll
│   ├── native/axr_runtime.lib  # bibliothèque d'import, pour lier depuis C/C++ MSVC ou cgo
│   ├── common/
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
├── linux-x64/       # Linux (x64)
│   ├── native/libaxr_runtime.so
│   ├── common/
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
├── linux-arm64/     # Linux (ARM64)
│   ├── native/libaxr_runtime.so
│   ├── common/
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
├── macos-arm64/     # macOS (Apple Silicon / arm64)
│   ├── native/libaxr_runtime.dylib
│   ├── common/
│   ├── python/
│   ├── csharp/
│   ├── vb/
│   └── java/
└── macos-x64/       # macOS (Intel / x64)
    ├── native/libaxr_runtime.dylib
    ├── common/
    ├── python/
    ├── csharp/
    ├── vb/
    └── java/
```

Chaque dossier est entièrement autonome (il ne dépend d'aucun dossier
d'une autre plateforme). L'utilisation est identique partout : consultez
le README à l'intérieur de chaque dossier `<plateforme>/<langage>/`.

## Plateformes prises en charge

| Dossier | État |
|---|---|
| `windows-x64/` | ✅ Vérifié (4 langages) |
| `windows-arm64/` | ✅ Vérifié (4 langages) |
| `linux-arm64/` | ✅ Vérifié (4 langages) |
| `linux-x64/` | ✅ Vérifié (4 langages) |
| `macos-arm64/` | ✅ Vérifié (4 langages) |
| `macos-x64/` | ✅ Vérifié (4 langages) |

Si vous ne disposez pas exactement de cette plateforme, vous pouvez
utiliser la variable d'environnement `ACR_NATIVE_LIB` pour indiquer
l'emplacement d'une bibliothèque obtenue par un autre moyen, et
réutiliser le même code d'exemple (pour Python/C#/VB.NET il s'agit du
chemin complet vers le fichier de bibliothèque, pour Java du chemin
vers le répertoire qui le contient).

## API exposée

Cet exemple n'utilise que l'ensemble minimal de fonctions suivant. Voir
[`acr.h`](acr.h) pour les déclarations C complètes (y compris
`acr_render_zip`).

| Fonction | Description |
|---|---|
| `acr_license_activate(email, key)` | Contacte le serveur de licences pour activer (connexion réseau requise) |
| `acr_license_last_message()` | Renvoie le message de la dernière tentative d'activation |
| `acr_render_pdf(templateJson, dataJson, &outLen)` | Génère un PDF sous forme de tableau d'octets |
| `acr_free_buffer(ptr, len)` | Libère le tampon renvoyé par les fonctions ci-dessus (l'appelant doit toujours l'appeler) |

Si vous souhaitez obtenir un ZIP (contenant un PNG par page, au format
ACR-PNG-PACKAGE) au lieu d'un PDF, il suffit d'appeler
`acr_render_zip(templateJson, dataJson, &outLen)`, qui prend les mêmes
arguments (cet exemple ne l'appelle pas).

## Lier depuis C / C++ (et cgo)

Le fichier `acr.h` à la racine du dépôt déclare l'interface C commune à
toutes les plateformes. Pour lier :

- Windows : liez `<platform>/native/axr_runtime.lib` (bibliothèque
  d'import) ; à l'exécution, placez `axr_runtime.dll` à côté de votre
  exécutable.
- Linux : liez directement `<platform>/native/libaxr_runtime.so` (aucune
  bibliothèque d'import séparée n'est nécessaire sous Linux).
- macOS : liez directement `<platform>/native/libaxr_runtime.dylib`.

Pour cgo, faites pointer `CGO_LDFLAGS` vers le dossier `native/` de la
plateforme (par ex. `-L${SRCDIR}/windows-x64/native -laxr_runtime` sous
Windows, pour le `.lib` ; directement vers le `.so`/`.dylib` sous
Linux/macOS) et faites `#include "acr.h"` dans le préambule cgo.

## Licence et filigrane

**Sans activation (comportement par défaut), le PDF généré comporte un
filigrane.** Ce filigrane ne disparaît qu'après activation avec une
licence ENGINE.

Vous pouvez obtenir gratuitement une clé de licence sur
[licensvr.acrossreport.com](https://licensvr.acrossreport.com)
(inscription possible via Google, GitHub ou une adresse e-mail, en
utilisant le formulaire d'inscription pour ACR Engine). Définissez la
clé obtenue dans `ACR_LICENSE_EMAIL` / `ACR_LICENSE_KEY`.

Chaque exemple n'appelle `acr_license_activate()` que si les deux
variables d'environnement `ACR_LICENSE_EMAIL` et `ACR_LICENSE_KEY` sont
définies (sinon, il affiche « activation: skipped » et continue sans
activation). **Vérifiez la présence du filigrane en ouvrant réellement
le fichier `output.pdf` généré.**

## Points d'attention pour l'intégration

- Le tampon renvoyé par `acr_render_pdf` (et `acr_render_zip`) est de
  la mémoire allouée par la bibliothèque. Une fois que vous avez fini
  de l'utiliser, vous devez impérativement appeler
  `acr_free_buffer(ptr, len)`. Si vous ne le faites pas, cela provoque
  une fuite mémoire (le système d'exploitation la récupère à la fin du
  processus, mais cet appel est obligatoire dans un processus serveur
  de longue durée).
- Au minimum, le tableau `ACR.Report.Sections.Section` du JSON de
  template doit contenir **exactement une section qui se normalise
  vers le nom « Detail ».** `common/template.json` en est la forme
  minimale.
