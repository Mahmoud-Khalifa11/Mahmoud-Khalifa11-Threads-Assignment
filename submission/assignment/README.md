# Threads Assignment



## Status



This submission is partial. Parts 01-05 are done as code. The `ANSWERS.md` files of Parts 01, 02, 04 and 05 are still the empty templates. Part 06 is only partly built.



| Part | Status |

|---|---|

| 01 Invoice Export | Code done (async, orders started first and awaited one by one). Run time 6.0 s -> 1.9 s. The 5 invoices are identical before and after (checked by file hash). |

| 02 Table Booking | Code done (`lock`). Original runs gave wrong totals (for example `Booked=29, TablesLeft=-28`). After the fix, 5 runs all gave `Booked=5, TablesLeft=0`. |

| 03 Locking Research | `ANSWERS.md` written (Q1-Q5 and links). |

| 04 Menu Views | Code done (`Interlocked.Increment`). Original runs gave 176279 to 272212 views. After the fix, 5 runs all gave 400000. |

| 05 Views and Emails | Code done (`ConcurrentDictionary.AddOrUpdate`, `ConcurrentQueue.TryDequeue`). The original code crashed in 6 of 8 runs. After the fix, 8 runs gave 50000 views and 20 unique emails. |

| 06 Mini Project | Partial: shared foundation (models, exceptions, `Log`, `JsonFileStore`, `DataSeeder`) and Phase 1 only. Phases 2, 3 and 4, the project README, the screenshots and `sample-output/` are not done. |



## Part 06, Phase 1



Run it with `dotnet run --project OrderProcessing -- phase1` from `06-OrderProcessing/`. The result is 20 orders: 15 Validated and 5 Rejected (#4, #9, #12, #15, #18).



Measured times:



| Run | Time |

|---|---|

| Sync (validation only, nothing saved) | 35.0 s |

| Async, validation only | 35.2 s |

| Async, saving to `orders.json` | 18.9 s |

| Async, total | 54.1 s |



Sync and async validation take almost the same time because each order is awaited before the next one starts, so the waiting adds up the same way. The async total is higher only because it also saves each order and the sync run does not.

