# Abuse Case Analyse voor GeoProfs Verlofregistratiesysteem

Dit document beschrijft potentiële misbruikscenario's (abuse cases) voor de kernfunctionaliteiten van het systeem.

---

### UC-01: Een medewerker dient een verlofaanvraag in

| Use Case | Potentieel Misbruik (Abuse Case) | Aanvaller | Motivatie | Mitigatie (Maatregel) |
|---|---|---|---|---|
| Medewerker dient verlof in | **AC-01.1: Vervalsen van gebruiker (Spoofing)** | Externe aanvaller / Malafide collega | Verlof aanvragen op naam van een ander om verwarring te stichten of de persoon in een kwaad daglicht te stellen. | Sterke authenticatie (JWT-token validatie). De `UserId` wordt uit het token gehaald, niet uit de request body. |
| Medewerker dient verlof in | **AC-01.2: SQL-injectie in 'Reden'-veld** | Externe aanvaller | Toegang krijgen tot de database, data stelen of vernietigen. | Input validatie en het gebruik van Prepared Statements (standaard in Entity Framework). |
| Medewerker dient verlof in | **AC-01.3: Denial of Service door massa-aanvragen** | Externe aanvaller | Het systeem onbeschikbaar maken voor legitieme gebruikers. | Rate Limiting op het API-endpoint (bv. max 10 aanvragen per minuut per gebruiker). |
| Medewerker dient verlof in | **AC-01.4: Negatief verlofsaldo forceren** | Interne gebruiker (medewerker) | Meer verlof opnemen dan toegestaan door een race condition of manipulatie. | Server-side validatie van het verlofsaldo binnen een databasetransactie. De berekening mag nooit op de client plaatsvinden. |

---

### UC-02: Een manager beoordeelt een verlofaanvraag

| Use Case | Potentieel Misbruik (Abuse Case) | Aanvaller | Motivatie | Mitigatie (Maatregel) |
|---|---|---|---|---|
| Manager beoordeelt aanvraag | **AC-02.1: Ongeautoriseerde goedkeuring (Elevation of Privilege)** | Medewerker / Externe aanvaller | Eigen verlofaanvraag goedkeuren door de API direct aan te roepen. | RBAC (Role-Based Access Control) op het endpoint. De backend moet controleren of de gebruiker de rol 'Manager' heeft. |
| Manager beoordeelt aanvraag | **AC-02.2: Goedkeuren van aanvraag buiten eigen team** | Interne gebruiker (Manager) | De aanvraag van een medewerker van een andere afdeling goedkeuren/afkeuren. | Resource-based authorization. De backend moet controleren of de `managerId` van de ingelogde gebruiker overeenkomt met de manager van de medewerker van de aanvraag. |

---

### UC-03 & UC-04: Inzien van data

| Use Case | Potentieel Misbruik (Abuse Case) | Aanvaller | Motivatie | Mitigatie (Maatregel) |
|---|---|---|---|---|
| Gebruiker bekijkt data | **AC-03.1: Inzien van andermans verlofsaldo (Information Disclosure)** | Interne gebruiker (medewerker) | Privacy-gevoelige informatie van collega's achterhalen. | De API moet altijd de `userId` van de ingelogde gebruiker gebruiken voor de query. Een `userId` als parameter toestaan is alleen toegestaan voor managers, met extra controle. |
| Gebruiker bekijkt data | **AC-04.1: Manager lekt data van heel team** | Interne gebruiker (Manager) | Data van het hele team exporteren/kopiëren voor oneigenlijk gebruik. | Geen directe maatregel, maar wel **logging (Accounting)**. Log alle bulk-dataverzoeken zodat verdachte activiteit kan worden gedetecteerd. |
