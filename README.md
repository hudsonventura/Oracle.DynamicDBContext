# DinamicDBContext 
DinamicDBContext class to provide a easy access to a DB, using SQL string query or statement, when you can't use a ORM. See documentation how to use.<br>
Yes, with I instead of Y.


I disclame for you that DinamicDBContext must be used in a simple project, or project that you can't use a ORM, either by the complexity, or just a simple things.

If you want to talk to me, for any purpose, send me an email. hudsonventura@outlook.com

<br>

### The using...

using HudsonVentura;<br>
<br>
<br>

### How to create a connection to database
```
var stringConnection = $"User Id=DBUSER;Password=DBPASS;Data Source=DBHOST:DBPORT/DNNAME";
var accessDB = new DinamicDBContext(new OracleConnection(stringConnection));
```

### How to make a simple SELECT query with just an entry<br>
```
string query = $"SELECT * FROM wfprocess WHERE p.fgstatus in (1, 4) AND coupaid = '{id}'";
var data = accessDB<MyClassToBind>queryOne(query);
```

### How to make a simple SELECT query with many entries<br>
```
string query = $" SELECT * FROM wfprocess WHERE p.fgstatus in (1, 4) AND rownum <= 10";
var data = accessDB<MyClassToBind>query(query);
```

### To prepare and query a statement<br>
```
In construction
```

### To execute a non query statement<br>
```
string query = $"UPDATE wfprocess set test = 'ok' WHERE coupaid = '{id}'";
var data = accessDB<MyClassToBind>execute(query);
```