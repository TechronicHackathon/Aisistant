using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace Aisistant.Data.Models;

public class SessionTranscript
{
    public string sessionID {get;set;}
    public string chatMessage {get;set;}
    public DateTime timestamp {get;set;}
    public int startT_S {get;set;}
    public int endT_S {get;set;}
    public bool stale {get;set;}
    public string type {get;set;}
}