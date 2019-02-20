# Prismadb-benchmark
[![Build status](https://ci.appveyor.com/api/projects/status/32r7s2skrgm9ubva?svg=true)](https://ci.appveyor.com/project/bazzilic/prismadb-benchmark)

Benchmark Test for PrismaDB.

This test is mainly to test the max RPS of SQL DML operations, including `INSERT, SELECT, UPDATE, DELETE`. Each operation is test by `SINGLE` and `MULTIPLE` modes respectively. `MULTIPLE` means operate with decades(default set is 10) records at one time using one query. For `SELECT`, use different modes(`SELECT_WITH_JOIN, SELECT_WITH_ADDITION, SELECT_WITH_MULTIPLICATION, SELECT_WITH_COMPOUND`) to evaluate.

Moreover, this test measures the consumed time of each cryptographic functions like `EN/DECRYPTION` of `STORE, SEARCH, RANGE, ADDITION, MULTIPLICATION` and `WILDCARD`. Each cryptographic operation is base on 10000 records within one field.
