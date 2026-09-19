using System.Collections.Generic;
using UnityEngine;

// Builds the nine documents of a passenger from the passenger's identity and hidden truth,
// plants the inconsistencies (anomalies) the player can find, and answers database queries.
public static class DocumentSystem
{
    public static string CurrentDate = "13.11.1998";
    public const int CurrentYear = 1998;

    public static string TitleOf(DocumentType t)
    {
        switch (t)
        {
            case DocumentType.Ticket:        return Loc.T("BILET", "TICKET");
            case DocumentType.Id:            return Loc.T("DOWÓD OSOBISTY", "ID CARD");
            case DocumentType.RailDatabase:  return Loc.T("BAZA KOLEJOWA", "RAILWAY DATABASE");
            case DocumentType.Registry:      return Loc.T("REJESTR", "REGISTRY");
            case DocumentType.Manifest:      return Loc.T("MANIFEST PASAŻERÓW", "PASSENGER MANIFEST");
            case DocumentType.Medical:       return Loc.T("KARTA MEDYCZNA", "MEDICAL RECORD");
            case DocumentType.DeathRecord:   return Loc.T("AKT ZGONU", "DEATH RECORD");
            case DocumentType.TravelHistory: return Loc.T("HISTORIA PODRÓŻY", "TRAVEL HISTORY");
            default:                         return Loc.T("ROZKAZ SPECJALNY", "SPECIAL ORDERS");
        }
    }

    public static string CodexIdFor(DocumentType t)
    {
        switch (t)
        {
            case DocumentType.Ticket:        return "doc_ticket";
            case DocumentType.Id:            return "doc_id";
            case DocumentType.RailDatabase:  return "doc_raildb";
            case DocumentType.Registry:      return "doc_registry";
            case DocumentType.Manifest:      return "doc_manifest";
            case DocumentType.Medical:       return "doc_medical";
            case DocumentType.DeathRecord:   return "doc_death";
            case DocumentType.TravelHistory: return "doc_travel";
            default:                         return "doc_orders";
        }
    }

    public static string CodexIdForAnomaly(string a)
    {
        switch (a)
        {
            case "a_dead": return "ano_dead";
            case "a_norecord": return "ano_nonex";
            case "a_birth": return "ano_date";
            case "a_date": return "ano_date";
            case "a_photo": return "ano_photo";
            case "a_signature": return "ano_signature";
            case "a_seal": return "ano_seal";
            case "a_amended": return "ano_altered";
            case "a_echo": return "ano_echo";
            case "a_unknown": return "ano_unknown";
            case "a_name": return "doc_ticket";
            case "a_manifest": return "doc_manifest";
            case "a_micro": return "lore_line13b";
            default: return null;
        }
    }

    static DocLine L(string pl, string en, string value, string anomaly = null)
    {
        return new DocLine(Loc.T(pl, en), value) { anomaly = anomaly };
    }

    static DocLine Hidden(string pl, string en, string value, string anomaly, int minSkill)
    {
        return new DocLine(Loc.T(pl, en), value) { anomaly = anomaly, hidden = true, minSkill = minSkill };
    }

    static string Rd(int lo, int hi) { return Random.Range(lo, hi).ToString(); }

    static string Date(int year) { return Random.Range(1, 29).ToString("00") + "." + Random.Range(1, 13).ToString("00") + "." + year; }

    static readonly string[] doctors = { "dr Nowicki", "dr Lis", "dr Baran", "dr Kwiatek", "dr Sowiński", "dr Wilk" };
    static readonly string[] blood = { "0 Rh+", "A Rh+", "B Rh+", "AB Rh+", "0 Rh-", "A Rh-" };
    static readonly string[] causes =
    {
        "zatrzymanie akcji serca|cardiac arrest", "wypadek na torach|accident on the tracks", "zapalenie płuc|pneumonia",
        "utonięcie|drowning", "nieznana|unknown", "mróz|frost"
    };
    static readonly string[] microLines =
    {
        "TRZYNASTA STACJA NIE MA POSTOJU|THE THIRTEENTH STATION HAS NO STOP",
        "TEN DOKUMENT JUŻ BYŁ UŻYTY|THIS DOCUMENT HAS ALREADY BEEN USED",
        "KONDUKTOR NIE WSIADŁ|THE CONDUCTOR DID NOT BOARD",
        "PAMIĘTAJ SWOJE IMIĘ|REMEMBER YOUR NAME",
        "PERON NIE MA KOŃCA|THE PLATFORM HAS NO END"
    };

    static string Split(string pair)
    {
        string[] p = pair.Split('|');
        return Loc.T(p[0], p.Length > 1 ? p[1] : p[0]);
    }

    // ------------------------------------------------------------------------------------

    public static void Build(PassengerData p, DayConfig day)
    {
        p.documents.Clear();
        p.anomalies.Clear();

        string bd = p.birthDate;
        string name = p.fullName;
        bool nonex = p.truth == PassengerTruth.NonExistent;
        bool nonexMissing = nonex && (p.birthYear % 2 == 0);        // variant B: no entry at all
        bool dead = p.truth == PassengerTruth.Dead;

        // ---------- ticket ----------
        var ticket = new DocumentData { type = DocumentType.Ticket, number = p.ticketNo, stamp = Loc.T("STACJA ", "STATION ") + p.origin.ToUpper() };
        if (p.noTicket)
        {
            ticket.front.Add(new DocLine(Loc.T("BILET", "TICKET"), Loc.T("BRAK BILETU", "NO TICKET")) { anomaly = "a_name" });
        }
        else
        {
            ticket.front.Add(L("IMIĘ I NAZWISKO", "NAME", p.ticketName, p.forgedTicket ? "a_name" : null));
            ticket.front.Add(L("TRASA", "ROUTE", p.origin + " → " + p.destination, null));
            ticket.front.Add(L("MIEJSCE", "SEAT", p.ticketText, null));
            ticket.front.Add(new DocLine(Loc.T("NR BILETU", "TICKET NO"), p.ticketNo) { isNumber = true });
            ticket.front.Add(new DocLine(Loc.T("DATA", "DATE"), day.date) { isDate = true });
            ticket.front.Add(new DocLine(Loc.T("KASJER", "CASHIER"), Pick(doctors).Replace("dr ", "")) { isSignature = true, anomaly = p.forgedTicket ? "a_signature" : null });
            if (p.forgedTicket)
                ticket.front.Add(Hidden("PIECZĘĆ", "SEAL", Loc.T("stacja: ", "station: ") + "Rzeszów", "a_seal", 2));
            ticket.back.Add(L("UWAGA", "NOTICE", Loc.T("Bilet ważny tylko jedną noc.", "Valid for one night only."), null));
            ticket.back.Add(L("REGULAMIN", "RULES", Loc.T("Kolej nie odpowiada za osoby bez wpisu.", "The railway is not liable for persons without an entry."), null));
        }
        p.documents.Add(ticket);

        // ---------- ID ----------
        int issueYear = Mathf.Min(CurrentYear - 1, p.birthYear + Random.Range(18, 30));
        string issued = Date(issueYear);
        var id = new DocumentData { type = DocumentType.Id, number = p.docNo, stamp = Loc.T("URZĄD ", "OFFICE ") + p.origin.ToUpper() };
        id.photoLook = p.idPhotoLook >= 0 ? p.idPhotoLook : p.look;
        id.photoIsPlayer = p.playerPhoto;
        id.front.Add(new DocLine(Loc.T("ZDJĘCIE", "PHOTO"), "") { isPhoto = true, anomaly = p.truth == PassengerTruth.Impostor ? "a_photo" : null });
        id.front.Add(L("IMIĘ I NAZWISKO", "NAME", name, null));
        id.front.Add(new DocLine(Loc.T("UR.", "BORN"), bd) { isDate = true });
        id.front.Add(new DocLine(Loc.T("NR DOKUMENTU", "DOCUMENT NO"), p.docNo) { isNumber = true });
        id.front.Add(new DocLine(Loc.T("WYDANO", "ISSUED"), issued) { isDate = true });
        id.front.Add(L("ZAMIESZKANIE", "RESIDENCE", p.origin, null));
        id.front.Add(new DocLine(Loc.T("PODPIS", "SIGNATURE"), name) { isSignature = true, anomaly = p.truth == PassengerTruth.Impostor ? "a_signature" : null });
        id.front.Add(L("STATUS", "STATUS", Loc.T("WAŻNY", "VALID"), null));
        id.back.Add(L("ORGAN WYDAJĄCY", "ISSUED BY", Loc.T("Urząd Ewidencji, ", "Records Office, ") + p.origin, null));
        id.back.Add(L("UWAGA", "NOTICE", Loc.T("Dokument należy zwrócić po zgonie właściciela.", "Return this document after the holder's death."), null));

        // ---------- railway database ----------
        int trips = Random.Range(2, 30);
        var db = new DocumentData { type = DocumentType.RailDatabase, number = "DB-" + Random.Range(1000, 9999), stamp = Loc.T("KOLEJ", "RAIL") + " 13" };
        db.front.Add(L("IMIĘ I NAZWISKO", "NAME", name, null));
        db.front.Add(new DocLine(Loc.T("UR.", "BORN"), bd) { isDate = true });
        db.front.Add(L("STATUS", "STATUS", Loc.T("AKTYWNY", "ACTIVE"), null));
        db.front.Add(L("OSTATNIA PODRÓŻ", "LAST TRIP", Rd(1990, CurrentYear), null));
        db.front.Add(L("LICZBA PODRÓŻY", "TRIPS", trips.ToString(), null));
        db.back.Add(L("UWAGA", "NOTE", Loc.T("Dane pasażera przesyłane ze stacji co noc.", "Passenger data is sent from the stations every night."), null));

        // ---------- registry ----------
        var reg = new DocumentData { type = DocumentType.Registry, number = "REJ-" + Random.Range(10000, 99999), stamp = Loc.T("REJESTR", "REGISTRY") };
        reg.front.Add(L("IMIĘ I NAZWISKO", "NAME", name, null));
        reg.front.Add(new DocLine(Loc.T("UR.", "BORN"), bd) { isDate = true });
        reg.front.Add(L("STATUS", "STATUS", Loc.T("ŻYJE", "ALIVE"), null));
        reg.front.Add(L("ADRES", "ADDRESS", p.origin + ", ul. " + Pick(new[] { "Długa", "Krótka", "Kolejowa", "Zimna", "Cicha" }) + " " + Random.Range(1, 60), null));
        reg.back.Add(L("UWAGA", "NOTE", Loc.T("Wpis urzędowy jest ostateczny.", "An official entry is final."), null));

        // ---------- manifest ----------
        var man = new DocumentData { type = DocumentType.Manifest, number = "M-" + Random.Range(100, 999), stamp = Loc.T("LISTA NOCNA", "NIGHT LIST") };
        man.front.Add(L("NA LIŚCIE", "LISTED", Loc.T("TAK", "YES"), null));
        man.front.Add(L("IMIĘ I NAZWISKO", "NAME", name, null));
        man.front.Add(L("WAGON", "CAR", Random.Range(1, 6).ToString(), null));
        man.front.Add(new DocLine(Loc.T("DATA", "DATE"), day.date) { isDate = true });
        man.back.Add(L("UWAGA", "NOTE", Loc.T("Wagon 13 nie figuruje w manifeście.", "Car 13 does not appear on the manifest."), null));

        // ---------- medical ----------
        int lastVisit = Random.Range(Mathf.Max(1990, p.birthYear + 2), CurrentYear);
        var med = new DocumentData { type = DocumentType.Medical, number = "MED-" + Random.Range(1000, 9999), stamp = Loc.T("PRZYCHODNIA", "CLINIC") };
        med.front.Add(L("PACJENT", "PATIENT", name, null));
        med.front.Add(new DocLine(Loc.T("UR.", "BORN"), bd) { isDate = true });
        med.front.Add(L("GRUPA KRWI", "BLOOD TYPE", Pick(blood), null));
        med.front.Add(L("ROZPOZNANIE", "DIAGNOSIS", p.illness, null));
        med.front.Add(new DocLine(Loc.T("OSTATNIA WIZYTA", "LAST VISIT"), lastVisit.ToString()) { isDate = true });
        med.front.Add(new DocLine(Loc.T("LEKARZ", "PHYSICIAN"), Pick(doctors)) { isSignature = true });
        med.back.Add(L("UWAGA", "NOTE", Loc.T("Karta nie zawiera informacji o pobycie w wagonie 13.", "The record contains nothing about stays in car 13."), null));

        // ---------- death record ----------
        var death = new DocumentData { type = DocumentType.DeathRecord, number = "ZG-" + Random.Range(1000, 9999), stamp = Loc.T("USC", "REGISTRY OFFICE") };
        death.front.Add(L("STATUS", "STATUS", Loc.T("BRAK AKTU ZGONU", "NO DEATH RECORD"), null));
        death.back.Add(L("UWAGA", "NOTE", Loc.T("Akt zgonu wystawia się w ciągu trzech dni.", "A death record is issued within three days."), null));

        // ---------- travel history ----------
        var travel = new DocumentData { type = DocumentType.TravelHistory, number = "PODR-" + Random.Range(100, 999), stamp = Loc.T("LINIA 13", "LINE 13") };
        int ty = CurrentYear - Random.Range(2, 5);
        for (int i = 0; i < 3; i++)
        {
            travel.front.Add(new DocLine(ty.ToString(), p.origin + " → " + Loc.T("Stacja ", "Station ") + Random.Range(1, 13)) { isDate = true });
            ty += Random.Range(0, 2);
        }
        travel.back.Add(L("UWAGA", "NOTE", Loc.T("Historia obejmuje tylko przejazdy Linią 13.", "History covers Line 13 journeys only."), null));

        // ---------- special orders ----------
        var orders = new DocumentData { type = DocumentType.SpecialOrders, number = "ROZ-" + Random.Range(100, 999), stamp = Loc.T("DYREKCJA", "DIRECTORATE") };
        orders.front.Add(L("ZASADA NOCY", "RULE OF THE NIGHT", day.rule, null));
        if (p.orderState == 0)
        {
            orders.front.Add(L("DLA PASAŻERA", "FOR THIS PASSENGER", Loc.T("brak rozkazów specjalnych", "no special orders"), null));
        }
        else
        {
            orders.front.Add(L("DLA PASAŻERA", "FOR THIS PASSENGER", Loc.T("WPUSZCZONY NA ROZKAZ DYREKCJI", "ADMITTED BY DIRECTORATE ORDER"), null));
            orders.front.Add(new DocLine(Loc.T("PODPIS", "SIGNATURE"), Loc.T("Dyr. J. Zawada", "Dir. J. Zawada")) { isSignature = true, anomaly = p.orderState == 2 ? "a_signature" : null });
            if (p.orderState == 2) orders.front.Add(Hidden("NR ROZKAZU", "ORDER NO", "ROZ-" + Random.Range(100, 999) + "/" + Loc.T("1987", "1987"), "a_date", 2));
        }
        orders.back.Add(L("UWAGA", "NOTE", Loc.T("Rozkaz bez pieczęci jest nieważny.", "An order without a seal is void."), null));

        // ================= plant the anomalies by truth =================
        switch (p.truth)
        {
            case PassengerTruth.Dead:
            {
                string dd = p.deathDate;
                Mark(reg, 2, Loc.T("ZMARŁ", "DECEASED"), "a_dead");
                reg.front.Add(L("DATA ZGONU", "DATE OF DEATH", dd, "a_dead"));
                Mark(db, 2, Loc.T("NIEAKTYWNY", "INACTIVE"), null);
                Mark(db, 3, p.deathYear.ToString(), null);
                Mark(med, 4, p.deathYear.ToString() + Loc.T(" (ostatni wpis)", " (final entry)"), null);
                death.front.Clear();
                death.front.Add(L("DATA ZGONU", "DATE OF DEATH", dd, "a_dead"));
                death.front.Add(L("PRZYCZYNA", "CAUSE", p.deathCause, null));
                death.front.Add(L("POCHOWANY", "BURIED", p.origin, null));
                travel.front.Add(new DocLine((p.deathYear + Random.Range(1, 4)).ToString(), p.origin + " → " + Loc.T("Stacja ", "Station ") + p.station) { isDate = true, anomaly = "a_date" });
                p.anomalies.Add("a_dead");
                break;
            }
            case PassengerTruth.NonExistent:
            {
                if (nonexMissing)
                {
                    Mark(reg, 2, Loc.T("BRAK WPISU", "NO ENTRY"), "a_norecord");
                    Mark(db, 2, Loc.T("BRAK WPISU", "NO ENTRY"), "a_norecord");
                    Mark(db, 3, Loc.T("BRAK", "NONE"), null);
                    Mark(db, 4, "0", null);
                    reg.front[1].value = "???";
                    Mark(med, 3, Loc.T("BRAK KARTY", "NO RECORD"), "a_norecord");
                    travel.front.Clear();
                    travel.front.Add(L("WPISY", "ENTRIES", Loc.T("BRAK", "NONE"), "a_norecord"));
                }
                else
                {
                    string wrong = Shift(bd, 1);
                    reg.front[1].value = wrong; reg.front[1].anomaly = "a_birth";
                    db.front[1].value = wrong; db.front[1].anomaly = "a_birth";
                    reg.front.Add(L("RODZICE", "PARENTS", Loc.T("zmarli w ", "deceased in ") + (p.birthYear - Random.Range(1, 4)), "a_birth"));
                }
                man.front[0].value = Loc.T("NIE", "NO"); man.front[0].anomaly = "a_manifest";
                p.anomalies.Add("a_norecord");
                break;
            }
            case PassengerTruth.Altered:
            {
                string changed = Shift(bd, 1);
                id.front[2].value = changed; id.front[2].anomaly = "a_amended";
                reg.front.Add(L("UWAGA", "NOTE", Loc.T("SPROSTOWANO ", "AMENDED ") + Date(CurrentYear), "a_amended"));
                id.front.Add(Hidden("SPROSTOWANIE", "AMENDMENT", Loc.T("nr ", "no. ") + Random.Range(100, 999) + Loc.T(" - zmiana daty ur.", " - birth date changed"), "a_amended", 3));
                p.anomalies.Add("a_amended");
                break;
            }
            case PassengerTruth.Loop:
            {
                int fut = CurrentYear + Random.Range(2, 30);
                id.front[4].value = Date(fut); id.front[4].anomaly = "a_date";
                Mark(db, 3, (fut + 1).ToString(), "a_date");
                travel.front.Add(new DocLine(fut.ToString(), p.origin + " → " + Loc.T("Stacja ", "Station ") + p.station) { isDate = true, anomaly = "a_date" });
                id.front.Add(Hidden("MIKROTEKST", "MICROTEXT", Split("TEN DOKUMENT JUŻ BYŁ UŻYTY|THIS DOCUMENT HAS ALREADY BEEN USED"), "a_micro", 2));
                p.anomalies.Add("a_date");
                break;
            }
            case PassengerTruth.Impostor:
            {
                // photo and signature belong to someone else (set when the ID was built)
                reg.front.Add(L("UWAGA", "NOTE", Loc.T("zgłoszono kradzież dokumentu", "document theft reported"), null));
                p.anomalies.Add("a_photo");
                break;
            }
            case PassengerTruth.Echo:
            {
                Mark(db, 2, Loc.T("PAMIĘĆ", "MEMORY"), "a_echo");
                db.front.Add(L("ŹRÓDŁO", "SOURCE", Loc.T("zgłoszenie świadków, nie osoby", "reported by witnesses, not by the person"), "a_echo"));
                travel.front.Add(new DocLine(CurrentYear.ToString(), Loc.T("z: ", "with: ") + Pick(new[] { "Anna Wolska", "Marek Sowa", "Piotr Wrona" })) { isDate = true });
                p.anomalies.Add("a_echo");
                break;
            }
            case PassengerTruth.Unknown:
            {
                Mark(db, 2, Loc.T("NIEZNANY", "UNKNOWN"), "a_unknown");
                Mark(reg, 2, "???", "a_unknown");
                death.front.Clear();
                death.front.Add(L("STATUS", "STATUS", Loc.T("NIE MOŻNA USTALIĆ", "UNABLE TO DETERMINE"), "a_unknown"));
                id.photoLook = -2;                                    // blank photo
                id.front[0].anomaly = "a_unknown";
                p.anomalies.Add("a_unknown");
                break;
            }
        }

        if (p.forgedTicket) p.anomalies.Add("a_name");
        if (p.noTicket) p.anomalies.Add("a_name");
        if (p.orderState == 2) p.anomalies.Add("a_signature");

        // no destination registered (only for those the system has trouble with)
        if (!p.destinationRegistered && !p.noTicket)
        {
            ticket.front[1].value = p.origin + " → " + Loc.T("BRAK ZAREJESTROWANEGO CELU", "NO REGISTERED DESTINATION");
            ticket.front[1].anomaly = "a_name";
        }

        // red herrings and lore: hidden microtext that means nothing for the decision
        if (Random.value < day.anomalyChance)
        {
            id.front.Add(Hidden("MIKROTEKST", "MICROTEXT", Split(microLines[Random.Range(0, microLines.Length)]), "a_micro", 2 + Random.Range(0, 3)));
        }
        if (p.truth == PassengerTruth.Alive && Random.value < day.anomalyChance * 0.5f)
        {
            reg.front.Add(L("UWAGA", "NOTE", Loc.T("rozmazana pieczęć (uszkodzenie papieru)", "smudged seal (paper damage)"), null));
        }

        // the conductor's own face on someone else's document (event) or conductor's own name
        if (p.playerPhoto)
        {
            id.photoIsPlayer = true;
            id.front[0].anomaly = "a_photo";
        }

        // the "Man in Grey" carries the conductor's name
        if (p.storyId != null && p.storyId.StartsWith("grey") && !string.IsNullOrEmpty(p.fullName))
        {
            id.front[3].value = "K-13-0013";
        }

        p.documents.Add(id);
        p.documents.Add(db);
        p.documents.Add(reg);
        p.documents.Add(man);
        p.documents.Add(med);
        p.documents.Add(death);
        p.documents.Add(travel);
        p.documents.Add(orders);

        // order by type so the array index equals the enum value
        p.documents.Sort((a, b) => ((int)a.type).CompareTo((int)b.type));
    }

    static void Mark(DocumentData d, int line, string value, string anomaly)
    {
        if (line < 0 || line >= d.front.Count) return;
        d.front[line].value = value;
        if (anomaly != null) d.front[line].anomaly = anomaly;
    }

    // changes the birth year of "dd.mm.yyyy" by delta years
    static string Shift(string date, int delta)
    {
        string[] parts = date.Split('.');
        if (parts.Length != 3) return date;
        int y;
        if (!int.TryParse(parts[2], out y)) return date;
        return parts[0] + "." + parts[1] + "." + (y + delta);
    }

    static string Pick(string[] a) { return a[Random.Range(0, a.Length)]; }

    public static string RandomIllness()
    {
        string[] ill =
        {
            "nadciśnienie|hypertension", "astma|asthma", "brak przewlekłych chorób|no chronic conditions",
            "bezsenność|insomnia", "reumatyzm|rheumatism", "zdrowy|healthy", "cukrzyca|diabetes"
        };
        return Split(ill[Random.Range(0, ill.Length)]);
    }

    public static string RandomCause() { return Split(causes[Random.Range(0, causes.Length)]); }

    public static DocumentData Get(PassengerData p, DocumentType t)
    {
        for (int i = 0; i < p.documents.Count; i++) if (p.documents[i].type == t) return p.documents[i];
        return null;
    }

    // Short text for the desk: only the visible lines of a document.
    public static string DeskText(DocumentData d)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("<b>").Append(TitleOf(d.type)).Append("</b>");
        int shown = 0;
        for (int i = 0; i < d.front.Count; i++)
        {
            DocLine l = d.front[i];
            if (l.hidden || l.isPhoto) continue;
            if (shown >= 6) break;
            sb.Append("\n").Append(l.label).Append(": ").Append(l.value);
            shown++;
        }
        return sb.ToString();
    }

    // ---------------- database terminal ----------------

    public const int QueryTicket = 0, QueryDocument = 1, QueryRelatives = 2, QueryHistory = 3, QueryIndex = 4;

    public static string QueryName(int q)
    {
        switch (q)
        {
            case QueryTicket:    return Loc.T("NUMER BILETU", "TICKET NUMBER");
            case QueryDocument:  return Loc.T("NUMER DOKUMENTU", "DOCUMENT NUMBER");
            case QueryRelatives: return Loc.T("KREWNI", "RELATIVES");
            case QueryHistory:   return Loc.T("HISTORIA LINII 13", "LINE 13 HISTORY");
            default:             return Loc.T("INDEKS ANOMALII", "ANOMALY INDEX");
        }
    }

    // Returns the text of the result. anomaly is set when the query reveals something.
    public static string Query(PassengerData p, int q, out string anomaly)
    {
        anomaly = null;
        int lvl = SkillSystem.Level(Skill.Documents);
        switch (q)
        {
            case QueryTicket:
                if (p.noTicket) { anomaly = "a_name"; return Loc.T("Brak biletu w systemie.", "No ticket in the system."); }
                if (p.forgedTicket) { anomaly = "a_name"; return Loc.T("BRAK ZGODNOŚCI. Bilet wystawiono na inne imię.", "NO MATCH. The ticket was issued in a different name."); }
                if (p.truth == PassengerTruth.NonExistent || p.truth == PassengerTruth.Unknown) { anomaly = "a_norecord"; return Loc.T("Bilet " + p.ticketNo + " wystawiony na: ??? (brak posiadacza)", "Ticket " + p.ticketNo + " issued to: ??? (no holder)"); }
                return Loc.T("Bilet " + p.ticketNo + " wystawiony na " + p.fullName + ". Ważny.", "Ticket " + p.ticketNo + " issued to " + p.fullName + ". Valid.");
            case QueryDocument:
                if (p.truth == PassengerTruth.Impostor) { anomaly = "a_photo"; return Loc.T("Dokument " + p.docNo + " należy do innej osoby. Zgłoszona kradzież.", "Document " + p.docNo + " belongs to another person. Theft reported."); }
                if (p.truth == PassengerTruth.NonExistent) { anomaly = "a_norecord"; return Loc.T("Brak rekordu dla numeru " + p.docNo + ".", "No record for number " + p.docNo + "."); }
                if (p.truth == PassengerTruth.Dead) { anomaly = "a_dead"; return Loc.T("Dokument " + p.docNo + " unieważniony w " + p.deathYear + " r.", "Document " + p.docNo + " revoked in " + p.deathYear + "."); }
                if (p.truth == PassengerTruth.Loop) { anomaly = "a_date"; return Loc.T("Dokument wydany w przyszłości. Data systemowa niezgodna.", "Document issued in the future. System date mismatch."); }
                if (p.truth == PassengerTruth.Altered) { anomaly = "a_amended"; return Loc.T("Dokument zmieniony ręcznie. Dane się nie zgadzają.", "Document changed by hand. Data does not match."); }
                if (p.truth == PassengerTruth.Unknown) { anomaly = "a_unknown"; return Loc.T("Błąd. Numer wskazuje sam na siebie.", "Error. The number points back to itself."); }
                return Loc.T("Dokument " + p.docNo + " zgodny z osobą " + p.fullName + ".", "Document " + p.docNo + " matches " + p.fullName + ".");
            case QueryRelatives:
                if (p.truth == PassengerTruth.NonExistent) { anomaly = "a_norecord"; return Loc.T("Matka: " + p.motherName + ", zm. " + p.motherDeathYear + ". Data jest wcześniejsza niż data urodzenia pasażera.", "Mother: " + p.motherName + ", d. " + p.motherDeathYear + ". This is earlier than the passenger's birth date."); }
                if (p.truth == PassengerTruth.Dead) return Loc.T("Matka: " + p.motherName + ", zm. " + (p.deathYear - Random.Range(1, 8)) + ".", "Mother: " + p.motherName + ", d. " + (p.deathYear - Random.Range(1, 8)) + ".");
                if (p.truth == PassengerTruth.Echo) { anomaly = "a_echo"; return Loc.T("Brak krewnych. Wspominany przez: " + p.motherName + ".", "No relatives. Remembered by: " + p.motherName + "."); }
                if (p.truth == PassengerTruth.Unknown) { anomaly = "a_unknown"; return Loc.T("Pokrewieństwo: K-13-0013 (konduktor). Pętla.", "Relation: K-13-0013 (conductor). Loop."); }
                if (p.storyId == "lena_1") return Loc.T("Matka: Anna Wolska, aktywna.", "Mother: Anna Wolska, active.");
                return Loc.T("Matka: " + p.motherName + ", aktywna w bazie.", "Mother: " + p.motherName + ", active in the database.");
            case QueryHistory:
                if (p.truth == PassengerTruth.NonExistent) { anomaly = "a_norecord"; return Loc.T("Brak przejazdów. Mimo to wagon: " + Random.Range(1, 6) + " był zarezerwowany.", "No journeys. Yet car " + Random.Range(1, 6) + " was reserved."); }
                if (p.truth == PassengerTruth.Dead) { anomaly = "a_date"; return Loc.T("Ostatni przejazd: " + p.deathYear + ". Kolejne " + Random.Range(1, 5) + " po zgonie.", "Last journey: " + p.deathYear + ". " + Random.Range(1, 5) + " more after death."); }
                if (p.truth == PassengerTruth.Loop) { anomaly = "a_date"; return Loc.T("Przejazdy z daty przyszłej: " + Random.Range(1, 4) + ".", "Journeys dated in the future: " + Random.Range(1, 4) + "."); }
                return Loc.T("Przejazdy Linią 13: " + Random.Range(1, 12) + ". Ostatni: dawno.", "Line 13 journeys: " + Random.Range(1, 12) + ". Last: long ago.");
            default:
            {
                int baseIdx = p.truth == PassengerTruth.Alive ? Random.Range(3, 15) : Random.Range(55, 96);
                int noise = Random.Range(-(11 - lvl) * 3, (11 - lvl) * 3 + 1);
                int idx = Mathf.Clamp(baseIdx + noise, 0, 100);
                return Loc.T("Indeks anomalii: " + idx + "%.", "Anomaly index: " + idx + "%.") + (lvl < 4 ? Loc.T(" (odczyt niepewny)", " (uncertain reading)") : "");
            }
        }
    }
}
