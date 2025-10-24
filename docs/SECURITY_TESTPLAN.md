# Security Testplan voor GeoProfs Verlofregistratiesysteem

**Versie:** 1.0
**Datum:** 2025-10-24

## 1. Introductie

Dit document beschrijft de teststrategie en de concrete testgevallen (test cases) om de beveiliging van het GeoProfs verlofregistratiesysteem te valideren. Het doel is om te verifiëren dat de geïmplementeerde security-maatregelen effectief zijn tegen de geïdentificeerde dreigingen uit de abuse case- en STRIDE-analyse.

## 2. Scope

**In Scope:**
- De Backend API (`GeoProfs.Api`)
- Authenticatie- en autorisatiemechanismen (JWT, RBAC)
- Inputvalidatie en bescherming tegen injectie-aanvallen
- Bescherming van data (at rest en in transit)

**Out of Scope:**
- Fysieke beveiliging van de datacenters
- Onderliggende cloud-infrastructuur (wordt vertrouwd op de cloud provider)
- Frontend UI-specifieke tests (bv. layout-bugs), tenzij ze een security-impact hebben.

## 3. Teststrategie

We hanteren een combinatie van de volgende testmethoden:
- **Handmatige Penetratietests:** Het simuleren van aanvallen op basis van de gedefinieerde testgevallen.
- **Geautomatiseerde Security Scans:** (Toekomst) Gebruik van tools zoals OWASP ZAP of Snyk om de applicatie te scannen op bekende kwetsbaarheden.
- **Code Reviews:** Focussen op security-aspecten tijdens het reviewen van Pull Requests.

## 4. Testgevallen

De volgende testgevallen zijn direct afgeleid van de geïdentificeerde dreigingen.

### Test Area 1: Authenticatie & Sessiemanagement

| Test ID | Test Case | Stappen | Verwacht Resultaat |
|---|---|---|---|
| AUTH-01 | **Ongeautoriseerde toegang (Geen Token)** | 1. Doe een `POST` request naar `/api/leaverequests` zonder `Authorization` header. | De API retourneert een `401 Unauthorized` statuscode. |
| AUTH-02 | **Ongeldig Token** | 1. Doe een `POST` request naar `/api/leaverequests` met een ongeldig/verlopen JWT in de `Authorization` header. | De API retourneert een `401 Unauthorized` statuscode. |
| AUTH-03 | **Token Diefstal Preventie** | 1. Log in en verkrijg een token. 2. Probeer dezelfde API-call te doen via HTTP i.p.v. HTTPS. | De verbinding wordt geweigerd. De API is alleen via HTTPS bereikbaar. |

### Test Area 2: Autorisatie (Toegangscontrole)

| Test ID | Test Case | Stappen | Verwacht Resultaat |
|---|---|---|---|
| AUTHZ-01 | **Elevation of Privilege (Medewerker -> Manager)** | 1. Log in als normale medewerker (zonder 'Manager' rol). 2. Verkrijg een token. 3. Doe een `POST` request naar een fictief manager-endpoint, bv. `/api/leaverequests/{id}/approve`. | De API retourneert een `403 Forbidden` statuscode. |
| AUTHZ-02 | **Inzien van andermans data (Horizontale Escalatie)** | 1. Log in als Medewerker A. 2. Doe een `GET` request naar `/api/leaverequests?userId={ID_van_Medewerker_B}`. | De API retourneert een `403 Forbidden` statuscode of een lege lijst. De data van Medewerker B wordt niet getoond. |
| AUTHZ-03 | **Beoordelen van aanvraag buiten eigen team** | 1. Log in als Manager A (team X). 2. Probeer een verlofaanvraag van een medewerker uit team Y goed te keuren. | De API retourneert een `403 Forbidden` of een specifieke business-foutmelding ("Niet geautoriseerd voor dit team"). |

### Test Area 3: Input Validatie & Injectie

| Test ID | Test Case | Stappen | Verwacht Resultaat |
|---|---|---|---|
| INP-01 | **SQL-injectie in 'Reden'-veld** | 1. Dien een verlofaanvraag in met de reden: `' OR 1=1; --`. | De aanvraag wordt succesvol opgeslagen. De letterlijke tekst `' OR 1=1; --` staat in de database, er wordt geen SQL uitgevoerd. |
| INP-02 | **Cross-Site Scripting (XSS) in 'Reden'-veld** | 1. Dien een verlofaanvraag in met de reden: `<script>alert('XSS')</script>`. | De aanvraag wordt opgeslagen. De opgeslagen tekst in de database is gesanitized (bv. `&lt;script&gt;alert('XSS')&lt;/script&gt;`). |
| INP-03 | **Ongeldige data invoer** | 1. Dien een verlofaanvraag in waarbij `endDate` voor `startDate` ligt. | De API retourneert een `400 Bad Request` statuscode met een duidelijke foutmelding. |

### Test Area 4: Denial of Service

| Test ID | Test Case | Stappen | Verwacht Resultaat |
|---|---|---|---|
| DOS-01 | **API Rate Limiting** | 1. Schrijf een script dat 20 `POST` requests naar `/api/authentication/token` stuurt binnen 10 seconden. | De eerste ~10 requests slagen. De daaropvolgende requests ontvangen een `429 Too Many Requests` statuscode. |

### Test Area 5: Informatielekken (Information Disclosure)

| Test ID | Test Case | Stappen | Verwacht Resultaat |
|---|---|---|---|
| INFO-01 | **Gevoelige data in foutmeldingen** | 1. Forceer een `500 Internal Server Error` (bv. door de database tijdelijk te stoppen). | De API retourneert een generieke foutmelding (bv. "Er is een interne serverfout opgetreden.") en geen stack trace of database details. |
| INFO-02 | **Gevoelige data in API-responses** | 1. Roep een endpoint aan dat een lijst van medewerkers teruggeeft. | De response bevat geen BSN, wachtwoord-hashes of andere gevoelige data. |
