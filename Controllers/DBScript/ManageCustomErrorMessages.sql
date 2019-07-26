/*classe 50*** errori seriea*/
exec sys.sp_addmessage 50013,16, 'Squadra già  presente.', 'us_english',false,  'replace'

exec sys.sp_addmessage 50014,16, 'Il giocatore non è libero.', 'us_english',false,  'replace'
exec sys.sp_addmessage 50016,16, 'Squadra già  presente nella stagione.', 'us_english',false,  'replace'
exec sys.sp_addmessage 50017,16, 'Ruolo già  presente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 50018,16, 'Relazione Giocatore Stagione Ruolo già  presente.', 'us_english',false,  'replace'

/*classe 51*** errori membership*/
exec sys.sp_addmessage 51001,16, 'Username o password errati.', 'us_english',false,  'replace'
exec sys.sp_addmessage 51002,16, 'Username già presente.', 'us_english',false,  'replace'

/*classe 52*** errori web*/
exec sys.sp_addmessage 52001,16, 'Funzione già presente per la pagina.', 'us_english',false,  'replace'
exec sys.sp_addmessage 52002,16, 'Pagina già presente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 52003,16, 'Pagina non trovata.', 'us_english',false,  'replace'

exec sys.sp_addmessage 52004,16, 'Ruolo già presente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 52005,16, 'Ruolo non trovato.', 'us_english',false,  'replace'

exec sys.sp_addmessage 52006,16, 'Ruolo_Pagina_Funzione già presente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 52007,16, 'User_Ruolo già presente.', 'us_english',false,  'replace'
/*classe 53*** errori susyleague*/
exec sys.sp_addmessage 53001,16, 'Stagione già presente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53002,16, 'Competizione esistente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53003,16, 'Giornata esistente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53004,16, 'Incontro esistente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53005,16, 'Incontro_giocatore_squadra esistente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53006,16, 'Incontro non trovato.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53007,16, 'Nome squadra esistente.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53008,16, 'Lo User ha già una squadra per la stagione.', 'us_english',false,  'replace'

exec sys.sp_addmessage 53009,16, 'Esiste già una finestra di mercato nella data indicata.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53010,16, 'Finestra di mercato non trovata.', 'us_english',false,  'replace'

exec sys.sp_addmessage 53011,16, 'Il giocatore non è libero.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53012,16, 'Il giocatore non appartiene alla squadra di origine.', 'us_english',false,  'replace'
exec sys.sp_addmessage 53013,16, 'La Fantasquadra non dispone di fondi sufficienti.', 'us_english',false,  'replace'

--select * from sys.messages where message_id > 50000

select * from seriea.giornate


message_id  language_id severity is_event_logged text
----------- ----------- -------- --------------- ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
50001       1033        16       0               Giocatore già presente
50001       1040        16       0               Giocatore già presente
50002       1033        16       0               Giocatore non trovato
50003       1033        16       0               Aggiornate più righe di quelle attese
50004       1033        16       0               Giornata già presente
50005       1033        16       0               Giornata non trovata.
50006       1033        16       0               Squadra già presente.
50007       1033        16       0               Stagione già presente.
50008       1033        16       0               Statistica già presente.
50009       1033        16       0               Statistica non trovata.
50010       1033        16       0               Voto già presente.
50011       1033        16       0               Voto non trovato.
50012       1033        16       0               Squadra o stagione non presenti.
50013       1033        16       0               Squadra già  presente.
50014       1033        16       0               Il giocatore non è libero.
50015       1033        16       0               Giornata già  presente nella stagione.
50016       1033        16       0               Squadra già  presente nella stagione.
50017       1033        16       0               Ruolo già  presente.
50018       1033        16       0               Relazione Giocatore Stagione Ruolo già  presente.
51001       1033        16       0               Username o password errati.
51002       1033        16       0               Username già presente.
52001       1033        16       0               Funzione già presente per la pagina.
52002       1033        16       0               Pagina già presente.
52003       1033        16       0               Pagina non trovata.
52004       1033        16       0               Ruolo già presente.
52005       1033        16       0               Ruolo non trovato.
52006       1033        16       0               Ruolo_Pagina_Funzione già presente.
52007       1033        16       0               User_Ruolo già presente.
53001       1033        16       0               Stagione già presente.
53002       1033        16       0               Competizione esistente.
53003       1033        16       0               Giornata esistente.
53004       1033        16       0               Incontro esistente.
53005       1033        16       0               Incontro_giocatore_squadra esistente.
53006       1033        16       0               Incontro non trovato.
53007       1033        16       0               Nome squadra esistente.
53008       1033        16       0               Lo User ha già una squadra per la stagione.
53009       1033        16       0               Esiste già una finestra di mercato nella data indicata.
53010       1033        16       0               Finestra di mercato non trovata.
53011       1033        16       0               Il giocatore non è libero.
53012       1033        16       0               Il giocatore non appartiene alla Fantasquadra di origine.
53013       1033        16       0               La Fantasquadra non dispone di fondi sufficienti.
53014       1033        16       0               Il giocatore appartiene ad una altra Fantasquadra.
53015       1033        16       0               Il giocatore non appartiene alla Fantasquadra.
530032      1033        16       0               Giornata esistente.