# Inventory Transfer Bot 
A simple invetory tramsfer bot for Splinterlands. 
It's role is claim DEC, SPS and Cards and leter sent them to one master account.

There is no subscription for the bot, it is taking 1% of DEC and SPS on every transfer. 
I am still considering the level of the dev donation but it will never be larger than 1%. 

This bot is still work in progress, and as such some things might change. 
I have an idea to improve thte UI and onboarding process but it depends on the free time I will get.

# Installation 
Download bot from [Releases page](https://github.com/functional-solutions/SplinterlandClaimBot/releases) it will contain the single executable file with config. Just  run the exe file on your machine.

# Configuratin
Currently there is not UI configuration so all change have to be done in config.yml and account.yml. Idea is that config.yml contains the overridable settings which cabn change with every release and *accounts.yml* contains the list of accounts. 

Create the **account.ym** file like: 
```
sentTo: 'main_account_name'  
accounts:
  - username: 'account1_name'
    postingKey: 'account1_posting_key'
    activeKey: 'account1_active_or_master_key'
  - username: 'account2_name'
    postingKey: 'account2_posting_key'
    activeKey: 'account2_active_or_master_key'
```

The option in coinfig.yml can be changed as well but the default should be fine

# To be done.
I want to improve the UI so there is no need to manually edit configuration file.  However I will keep it as a console up so it can be run anywhere. 

Please report any bug so I can fix it. 

In general app is still in progress and will have to be updated. 

# Donation 
Event the bot it taking some donations, there is an option to help me more:
 * DEC in game in account @splinterbots 
 * BNB/ETH/MATIC 0x9761a8520ae18EA851544A17905256D0c3AEc688

# Discord 
[Discord](https://discord.gg/N5SqVBTe)

# Build status
[![Build Status](https://dev.azure.com/be-functional/Splinterbots/_apis/build/status/ClaimBot?branchName=master)](https://dev.azure.com/be-functional/Splinterbots/_build/latest?definitionId=46&branchName=master)
