﻿using Microsoft.Win32;
using MovieNight.Domain.Entities;
using MovieNight.Domain.Entities.MailE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.Domain.Entities.PersonalP;
using MovieNight.Domain.Entities.UserId;

namespace MovieNight.BusinessLogic.Core
{
    public class MailCore
    {
        public List<InboxD> InboxEquipmentFromData (int? userId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<MailDbTable,InboxD>()
                    .ForMember(dest => dest.SenderName, 
                        opt=>opt.MapFrom(src 
                            => src.Sender.UserName));
                c.CreateMap<MailDbTable,InboxD>()
                    .ForMember(dest => dest.RecipientName, 
                        opt=>opt.MapFrom(src 
                            => src.Recipient.UserName));
            });
            
            var mapper = config.CreateMapper();
            using (var db = new UserContext())
            {
                try
                {
                    var existsInMailDb = db.MailE.Where(f => f.RecipientId == userId);
                    var listOfMessages = mapper.Map<List<InboxD>>(existsInMailDb);
                    return listOfMessages;
                }
                catch (Exception exception)
                {
                    return null;
                }
            }
        }

        public bool SetAddMessageDb(InboxD message)
        {
            using (var db = new UserContext())
            {
                try
                { 
                    var messageTable = new MailDbTable()
                        {
                            SenderId = db.UsersT.FirstOrDefault(f=>f.UserName==message.SenderName)?.Id,
                            RecipientId = db.UsersT.FirstOrDefault(g=>g.UserName==message.RecipientName)?.Id,
                            Date = message.Date,
                            IsStarred = message.IsStarred,
                            Message = message.Message,
                            Theme = message.Theme
                        };
                        db.MailE.Add(messageTable);
                        db.SaveChanges();
                        return true;
                }
                catch (Exception exception)
                {
                    return false;
                }
            }
        }
        
        public List<InboxD> InboxSentFromData (int? userId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<MailDbTable,InboxD>()
                    .ForMember(dest => dest.SenderName, 
                        opt=>opt.MapFrom(src 
                            => src.Sender.UserName));
                c.CreateMap<MailDbTable,InboxD>()
                    .ForMember(dest => dest.RecipientName, 
                        opt=>opt.MapFrom(src 
                            => src.Recipient.UserName));
            });
            
            var mapper = config.CreateMapper();
            using (var db = new UserContext())
            {
                try
                {
                    var existsInMailDb = db.MailE.Where(f => f.SenderId == userId );
                    var listOfMessages = mapper.Map<List<InboxD>>(existsInMailDb);
                    return listOfMessages;
                }
                catch (Exception exception)
                {
                    return null;
                }
            }
        }
        
        public List<InboxD> InboxStarredFromData (int? userId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<MailDbTable,InboxD>()
                    .ForMember(dest => dest.SenderName, 
                        opt=>opt.MapFrom(src 
                            => src.Sender.UserName));
                c.CreateMap<MailDbTable,InboxD>()
                    .ForMember(dest => dest.RecipientName, 
                        opt=>opt.MapFrom(src 
                            => src.Recipient.UserName));
            });
            
            var mapper = config.CreateMapper();
            using (var db = new UserContext())
            {
                try
                {
                    var existsInMailDb = db.MailE.Where(f => f.RecipientId == userId && f.IsStarred==true);
                    var listOfMessages = mapper.Map<List<InboxD>>(existsInMailDb);
                    return listOfMessages;
                }
                catch (Exception exception)
                {
                    return null;
                }
            }
        }
    }
}
