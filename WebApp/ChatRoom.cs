//using DotNetify.Elements;
using DotNetify;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using UAParser;

namespace WebApp
{
  //public class ChatRoomExample : BaseVM
  //{
  //   public ChatRoomExample()
  //   {
  //      var markdown = new Markdown("DotNetify.DevApp.Docs.Examples.ChatRoom.md");

  //      AddProperty("ViewSource", markdown.GetSection(null, "ChatRoomVM.cs"))
  //         .SubscribeTo(AddInternalProperty<string>("Framework").Select(GetViewSource));

  //      AddProperty("ViewModelSource", markdown.GetSection("ChatRoomVM.cs"));
  //   }

  //   private string GetViewSource(string framework)
  //   {
  //      return framework == "Vue" ?
  //         new Markdown("DotNetify.DevApp.Docs.Vue.Examples.ChatRoom.md") :
  //         new Markdown("DotNetify.DevApp.Docs.Examples.ChatRoom.md").GetSection(null, "ChatRoomVM.cs");
  //   }
  //}

  public class ChatMessage
  {
    public int Id { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Text { get; set; }
  }

  public class ChatUser
  {
    private static int _counter = 0;

    public string Id { get; set; }
    public string CorrelationId { get; set; }
    public string Name { get; set; }
    public string IpAddress { get; set; }
    public string Browser { get; set; }

    public ChatUser(IConnectionContext connectionContext, string correlationId)
    {
      Id = connectionContext.ConnectionId;
      CorrelationId = correlationId;
      Name = $"user{Interlocked.Increment(ref _counter)}";
      IpAddress = connectionContext.HttpConnection.RemoteIpAddress.ToString();

      var browserInfo = Parser.GetDefault().Parse(connectionContext.HttpRequestHeaders.UserAgent);
      if (browserInfo != null)
        Browser = $"{browserInfo.UA.Family}/{browserInfo.OS.Family} {browserInfo.OS.Major}";
    }
  }

  public class ChatRoomVM : MulticastVM
  {
    private readonly IConnectionContext _connectionContext;
    private readonly string _vmInstanceId;
    private readonly IConfiguration _configuration;
    private readonly IDisposableClass _disposableClass;
    private readonly Random rnd = new Random();

    public string TestValue
    {
      get => Get<string>() ?? "";
      set
      {
        Set(value);
        Changed(() => TestValue);
      }
    }

    [ItemKey(nameof(ChatMessage.Id))]
    public List<ChatMessage> Messages { get; } = new List<ChatMessage>();

    [ItemKey(nameof(ChatUser.Id))]
    public List<ChatUser> Users { get; } = new List<ChatUser>();



    public Action<ChatMessage> SendMessage => chat =>
    {
      if (_disposableClass.IsDisposed)
      {
        throw new Exception("SendMessage: _disposableClass.IsDisposed");
      }

      string userId = _connectionContext.ConnectionId;
      chat.Id = Messages.Count + 1;
      chat.UserId = userId;
      chat.UserName = UpdateUserName(userId, chat.UserName);

      string[] testvalues = _disposableClass.Configuration.GetSection("TestValues").GetChildren().ToArray().Select(c => c.Value).ToArray();

      TestValue = testvalues[rnd.Next(0,testvalues.Length-1)] + $":{DateTime.Now}";


      var privateMessageUser = Users.FirstOrDefault(x => chat.Text.StartsWith($"{x.Name}:"));
      if (privateMessageUser != null)
        base.Send(new List<string> { privateMessageUser.Id, userId }, "PrivateMessage", chat);
      else
      {
        lock (Messages)
        {
          Messages.Add(chat);
          this.AddList(nameof(Messages), chat);
        }
      }
    };



    public Action<string> AddUser => correlationId =>
    {

      if (_disposableClass.IsDisposed)
      {
        throw new Exception("AddUser: _disposableClass.IsDisposed");
      }

      string[] testvalues = _disposableClass.Configuration.GetSection("TestValues").GetChildren().ToArray().Select(c => c.Value).ToArray();

      TestValue = testvalues[rnd.Next(0, testvalues.Length - 1)] + $" @ {DateTime.Now}";

      var user = new ChatUser(_connectionContext, correlationId);
      lock (Users)
      {
        Users.Add(user);
        this.AddList(nameof(Users), user);
      }
    };

    public Action RemoveUser => () =>
    {
      lock (Users)
      {
        var user = Users.FirstOrDefault(x => x.Id == _connectionContext.ConnectionId);
        if (user != null)
        {
          Users.Remove(user);
          this.RemoveList(nameof(Users), user.Id);
        }
      }
    };

    public ChatRoomVM(IConnectionContext connectionContext, IConfiguration configuration, IDisposableClass disposableClass, string vmInstanceId)
    {
      _connectionContext = connectionContext;
      _vmInstanceId = vmInstanceId;
      _configuration = configuration;
      _disposableClass = disposableClass;
    }

    //public override void Dispose()
    //{
    //  RemoveUser();
    //  PushUpdates();
    //  base.Dispose();
    //}
    protected override void Dispose(bool disposing)
    {
      RemoveUser();
      PushUpdates();
      base.Dispose(disposing);
    }

    private string UpdateUserName(string userId, string userName)
    {
      lock (Users)
      {
        var user = Users.FirstOrDefault(x => x.Id == userId);
        if (user != null)
        {
          if (!string.IsNullOrEmpty(userName))
          {
            user.Name = userName;
            this.UpdateList(nameof(Users), user);
          }
          return user.Name;
        }
      }
      return userId;
    }
  }
}