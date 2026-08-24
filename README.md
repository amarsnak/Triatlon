# Triatlon – sistem za upravljanje triatlonskih rezultatov

Celovit sistem za uvoz, upravljanje in prikaz rezultatov triatlonskih tekmovanj 
(IRONMAN, IRONMAN 70.3, ultra-triatlon). Sistem sestavljajo štiri komponente, 
ki si delijo skupno PostgreSQL bazo: konzolni uvoznik podatkov, namizna 
administracijska aplikacija (WPF) z namestitvenim programom in spletna aplikacija.

Razvito pri predmetu OZRA na FERI.

## Arhitektura

​```mermaid
flowchart TD
    CSV[CSV datoteke] --> Uvoz[Uvoz podatkov - konzola]
    Uvoz --> DB[(PostgreSQL baza)]
    DB --> Admin[Admin aplikacija - WPF + Installer]
    DB --> Splet[Spletna aplikacija - Razor Pages]
​```
## Komponente

### 1. Uvoz podatkov (konzolna aplikacija)
- Branje CSV datotek z rezultati tekmovanj (CsvHelper)
- Čiščenje podatkov: obravnava manjkajočih vrednosti, pretvorba časov
- Samodejno prepoznavanje tipa tekmovanja iz strukture map
- Uvoz v normalizirano PostgreSQL bazo (Npgsql) z merjenjem statistike

### 2. Baza podatkov (PostgreSQL)
Normaliziran model: `tekmovanje`, `tekmovalec`, `kategorija`, `rezultat`, `uporabnik`.
Rezultati vsebujejo delne čase (plavanje, T1, kolesarjenje, T2, tek) in skupni čas.

### 3. Administracijska aplikacija (WPF)
- Namizna aplikacija po vzorcu MVVM
- Urejanje tekmovalcev, tekmovanj in uporabnikov (CRUD)
- Prijava uporabnikov z vlogami in hashiranimi gesli
- Dvojezični vmesnik (slovenščina / angleščina)
- Namestitveni program (MSIX paket)

### 4. Spletna aplikacija (ASP.NET Core Razor Pages)
- Prikaz tekmovanj in rezultatov
- Komunikacija prek API storitve
- Upravljanje sej (30-min timeout), HTTPS, HSTS

## Tehnologije
- C# / .NET
- WPF (namizna aplikacija, MVVM)
- ASP.NET Core Razor Pages (splet)
- PostgreSQL + Npgsql
- CsvHelper (uvoz podatkov)
- MSIX (namestitveni paket)

## Zagon

### Baza
1. Ustvari PostgreSQL bazo `triathlon`
2. Zaženi skripto za ustvarjanje tabel <!-- dodaj pot do .sql skripte -->

### Uvoz podatkov
1. Nastavi connection string prek okoljske spremenljivke ali `appsettings.json`
2. Nastavi pot do mape z CSV datotekami
3. Zaženi konzolni projekt

### Admin aplikacija
Zaženi namestitveni program iz `Naloga3_Installer/AppPackages/` ali projekt v Visual Studio.

### Spletna aplikacija
```bash
dotnet run
```

## Avtor
Aleks Maršnak – študent ITK, FERI Univerza v Mariboru
