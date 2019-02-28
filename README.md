# Prismadb-benchmark
[![Build status](https://ci.appveyor.com/api/projects/status/32r7s2skrgm9ubva?svg=true)](https://ci.appveyor.com/project/bazzilic/prismadb-benchmark)

Benchmark Test for PrismaDB.

This test is mainly to test the max RPS of SQL DML operations and the consumed time of each cryptographic functions.

Two datasets (t1 & t2) are generated before doing the test, one is for SQL DML operations as there are some `DELETE` and `INSERT` queries which will change the size of dataset. Another one is created for cryptographic functions as the number of records always stays the same.

**CREATE Query:**
```sql
CREATE TABLE t1
                (
                    a INT ENCRYPTED FOR(MULTIPLICATION, ADDITION, SEARCH, RANGE),
                    b INT ENCRYPTED FOR(ADDITION),
                    c INT ENCRYPTED FOR(MULTIPLICATION),
                    d INT,
                    e VARCHAR(30) ENCRYPTED FOR(STORE)
                );
CREATE TABLE t2
                (
                    a INT ENCRYPTED FOR(STORE),
                    b INT,
                    c INT,
                    d INT,
                    e VARCHAR(30)
                );
```
Then DataTable in proxy looks like:

| a  |  b | c  |  d |  e |
| ------------ | ------------ | ------------ | ------------ | ------------ |
|  1 |  2 | 3  | 4  | b  |
|  4 |  5 | 6  | 7  | d  |
|  ... |  ... | ...  | ...  | ...  |

And DataTable in database looks like:

| rowId |  a.Paillier.Enc | a.ElGamal.Enc  | a.Range  | a.Fingerprint  | b.Paillier.Enc  |  c.ElGamal.Enc | d  | e.Store.Enc  |  Common.Paillier.N | Common.ElGamal.N |
| ------------ | ------------ | ------------ | ------------ | ------------ | ------------ | ------------ | ------------ | ------------ | ------------ | ------------ |
| 1 | 0x323...  | 0x52A...   |  2544... | 1235  | 0x323...  | 0x52A...  | 123  | 0xD7A...  | 0x895...  | 0x0D5...  |
| 2 | 0x673...  | 0x5A5...   |  2544... | 5454  | 0x389...  | 0x4D2...  | 124  | 0xE21...  | 0x895...  | 0x0D5...  |
| ... | ...  | ...   |  ... | ...  | ...  | ...  | ...  | ...  |  ... | ... |


For `INSERT, SELECT, UPDATE, DELETE`, each operation is test by `SINGLE` and `MULTIPLE` modes respectively. `MULTIPLE` means operating with mutiple(default set is 10) records at one time using one query. For DML-Test dataset, size of dataset is set to `{10k, 100k, 1m}` (by inserting records using `INSERT` query) where `4/10` is for `SINGLE` operations while `6/10` is for `MULTIPLE`.

**INSERT Query:**
```sql
INSERT INTO t1 (a,b,c,d,e) VALUES (1, 2, 3, 4, b);
INSERT INTO t1 (a,b,c,d,e) VALUES (1, 2, 3, 4, b), (1, 2, 3, 4, b), (1, 2, 3, 4, b), (1, 2, 3, 4, b), ...;
```

**SELECT Query:**
```sql
SELECT TOP 1 * FROM t1; --Select Limit One Without Where Clause
SELECT d FROM t1 WHERE d=1; --Select Without Encryption
SELECT a FROM t1 WHERE a=1; --Simple Select
SELECT a + b FROM t1 WHERE a=1; --Select with Addition
SELECT a * c FROM t1 WHERE a=1; --Select with Multiplication
SELECT a + a * c + b FROM t1 WHERE a=1; --Select with Compound
SELECT t1.a, t1.b, t2.d, t2.e FROM t1 INNER JOIN t2 ON t1.a = t2.a WHERE t1.a=1; --Select with Join
```

**UPDATE Query:**
```sql
UPDATE t1 SET a=1, b=2, c=3, d=4, e="b" WHERE a=1;
```

**DELETE Query:**
```sql
DELETE FROM t1 WHERE a=1;
```

Time evaluation for cryptographic functions inculdes `EN/DECRYPTION` of `STORE`, `SEARCH`, `RANGE`, `ADDITION`, `MULTIPLICATION` and `WILDCARD`. For each operation, a check query is executed to check the status of EN/DECRYPTION.

**EN/DECRYPTION Query:**
```sql
PRISMADB ENCRYPT t2.a FOR(STORE); --Encrypt for Store
PRISMADB ENCRYPT t2.a FOR(STORE) STATUS; --Check Status
PRISMADB DECRYPT t2.a; --Decrypt from Store
PRISMADB DECRYPT t2.a STATUS; --Check Status
```
