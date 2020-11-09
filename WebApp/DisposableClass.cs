using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebApp
{


  public interface IDisposableClass : IDisposable
  {
    bool IsDisposed { get; }
    IServiceProvider ServiceProvider { get; }
    public IConfiguration Configuration { get; }
  }

  public class DisposableClass : IDisposableClass
  {
    public DisposableClass(IServiceProvider serviceProvider,IConfiguration configuration)
    {
      ServiceProvider = serviceProvider;
      Configuration = configuration;
    }

    // Public implementation of Dispose pattern callable by consumers.
    public void Dispose()
    {
      Dispose(true);
      //GC.SuppressFinalize(this);
    }

    public bool IsDisposed { get; private set; } = false;
    public IServiceProvider ServiceProvider { get; private set; }
    public IConfiguration Configuration { get; private set; }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing)
    {
      if (IsDisposed)
        return;

      if (disposing)
      {
        //handle.Dispose();
        // Free any other managed objects here.
        //
        ServiceProvider = null;
      }

      IsDisposed = true;
    }
  }
}
