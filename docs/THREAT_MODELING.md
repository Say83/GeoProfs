# THREAT Modeling (STRIDE) voor GeoProfs Verlofregistratiesysteem

Dit document analyseert potentiële dreigingen voor de hoofdcomponenten van het systeem volgens de STRIDE-methodologie.

---

### Component 1: Backend API (`GeoProfs.Api`)

| STRIDE Categorie | Dreiging | Voorbeeld-scenario | Mitigatie (Maatregel) |
|---|---|---|---|
| **S**poofing | Een gebruiker doet zich voor als een ander. | Een aanvaller steelt een JWT-token en gebruikt dit om acties uit te voeren op naam van het slachtoffer. | Gebruik kortlevende JWT-tokens (bv. 15 min). Implementeer een refresh token-mechanisme. Gebruik HTTPS overal om token-diefstal te voorkomen. |
| **T**ampering | Data wordt tijdens transport gemanipuleerd. | Een aanvaller onderschept een verlofaanvraag en verandert de datums voordat deze de server bereikt (Man-in-the-Middle). | Forceer het gebruik van HTTPS (TLS 1.2/1.3) voor alle API-communicatie. |
| **R**epudiation | Een gebruiker ontkent een actie te hebben uitgevoerd. | Een manager keurt een aanvraag af en beweert later dat hij dit nooit heeft gedaan. | Implementeer een gedetailleerde **audittrail**. Log elke statuswijziging van een `LeaveRequest` met `who` (ManagerId), `what` (RequestId, nieuwe status) en `when` (timestamp). |
| **I**nformation Disclosure | Gevoelige informatie wordt gelekt. | Een fout in de API geeft per ongeluk de verlofredenen van alle medewerkers terug in een lijst-endpoint. | Implementeer DTOs (Data Transfer Objects) die alleen de strikt noodzakelijke data bevatten. Voer security code reviews uit. |
| **D**enial of Service | De API wordt onbereikbaar gemaakt. | Een botnet bestookt het login- of aanvraag-endpoint met duizenden requests per seconde. | Implementeer Rate Limiting en gebruik een Web Application Firewall (WAF) of een cloud-native DDoS-beschermingsservice. |
| **E**levation of Privilege | Een gebruiker krijgt meer rechten dan toegestaan. | Een normale medewerker vindt een manier om het endpoint voor het goedkeuren van aanvragen aan te roepen. | Strikte Role-Based Access Control (RBAC) op alle endpoints die manager-functionaliteit bevatten. Valideer de rol-claim in het JWT-token bij elk request. |

---

### Component 2: Database (`PostgreSQL`)

| STRIDE Categorie | Dreiging | Voorbeeld-scenario | Mitigatie (Maatregel) |
|---|---|---|---|
| **S**poofing | De applicatie maakt verbinding met een malafide database. | DNS-poisoning waardoor de API data naar een database van een aanvaller stuurt. | Gebruik versleutelde databaseverbindingen (SSL/TLS) en valideer het certificaat van de databaseserver. |
| **T**ampering | Data wordt direct in de database gemanipuleerd. | Een aanvaller met SQL-toegang wijzigt een verlofsaldo direct in de tabel. | Beperk database-toegang tot alleen de applicatie-account. Gebruik geen 'shared' accounts. Overweeg het gebruik van checksums of digitale handtekeningen op belangrijke rijen. |
| **R**epudiation | Acties in de database kunnen niet worden herleid. | Een DBA (Database Administrator) verwijdert handmatig gevoelige logbestanden. | Gebruik de ingebouwde audit-mogelijkheden van PostgreSQL (bv. pgaudit). Stuur database-logs naar een apart, read-only logsysteem. |
| **I**nformation Disclosure | Een aanvaller leest de volledige database-backup. | Een gestolen backup-bestand wordt offline geopend en alle BSN-nummers en verlofredenen worden gelezen. | Versleutel de database-backups. Versleutel specifiek gevoelige kolommen (zoals BSN) in de database zelf (Column-level encryption). |
| **D**enial of Service | De database wordt overbelast. | Een inefficiënte query (bv. een `SELECT *` op een enorme tabel zonder `WHERE`-clausule) wordt continu uitgevoerd. | Optimaliseer alle queries. Gebruik indexen op veelgebruikte kolommen. Monitor de database-performance. |
| **E**levation of Privilege | Een database-gebruiker krijgt te veel rechten. | De applicatie-account heeft `SUPERUSER`-rechten, waardoor een SQL-injectie catastrofale gevolgen kan hebben. | Pas het **Principle of Least Privilege** toe. De applicatie-account mag alleen `SELECT`, `INSERT`, `UPDATE`, `DELETE` rechten hebben op de benodigde tabellen, en niets meer. |
