using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

public class Account
{
    public string Id;
    public string Pw;
}

public class AccountContext:DbContext
{
    public DbSet<Account> Accounts{get;set;}

    public AccountContext()
    {
        _dbPath = "bin/account.db";
    }
    string _dbPath;
    public AccountContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_dbPath}");
    }

    public void Register(Account account)
    {
        var c = Accounts.Find(account.Id);
        if(c!=null)
            throw new System.ArgumentException();

        this.Add(account);
        this.SaveChanges();
    }

    public bool IsExists(Account account)
    {
        var c = Accounts.Find(account.Id);
        return c!=null && c.Pw == account.Pw;
    }

    public bool Update(Account account)
    {
        var c = Accounts.Find(account.Id);
        if(c!=null && c.Pw == account.Pw)
        {
            c.Pw = account.Pw;
            this.SaveChanges();
            return true;
        }

        return false;
    }
}