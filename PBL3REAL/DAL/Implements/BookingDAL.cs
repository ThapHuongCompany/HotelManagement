﻿

using HotelManagement.Extention;
using Microsoft.EntityFrameworkCore;
using PBL3REAL.DAL.Interfaces;
using PBL3REAL.Model;
using PBL3REAL.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PBL3REAL.DAL
{
    public class BookingDAL : IBookingDAL
    {
       
        public BookingDAL()
        {
        }

        public List<Booking> FindByProperties(int start, int length,CalendarVM searchByDate ,string search ,string orderby,string status)
        {
            var predicate = PredicateBuilder.True<Booking>();
            if (search != "") 
               predicate = predicate.And(x => x.BookCode.Contains(search) || x.BookIdclientNavigation.CliCode.Contains(search) || x.BookIduserNavigation.UserCode.Contains(search));
            if (searchByDate.type.Equals("Booked Date"))
            {
                predicate = predicate.And(x => x.BookBookdate >= searchByDate.fromDate && x.BookBookdate <= searchByDate.toDate);
            }
           
            if (searchByDate.type.Equals("Due Date"))
            {
                predicate = predicate.And(x => x.BookDuedate >= searchByDate.fromDate && x.BookDuedate <= searchByDate.toDate);
            }
          
            if (searchByDate.type.Equals("Checkin Date"))
            {
                predicate = predicate.And(x => x.BookCheckindate >= searchByDate.fromDate && x.BookCheckindate <= searchByDate.toDate);
            }
            if(searchByDate.type.Equals("Checkout Date"))
            {
                predicate = predicate.And(x => x.BookCheckoutdate >= searchByDate.fromDate && x.BookCheckoutdate <= searchByDate.toDate);
            }
            if (status != "All")
                predicate = predicate.And(x => x.BookStatus == status);
            IQueryable<Booking> query = AppDbContext.Instance.Bookings
                                .Include(x => x.BookIdclientNavigation)
                                .Include(x => x.BookIduserNavigation)
                                .Where(predicate);                             
            switch (orderby)
            {
                case "None": break;
                case "Total Price Asc": query = query.OrderBy(x => x.BookTotalprice); break;
                case "Total Price Desc": query = query.OrderByDescending(x => x.BookTotalprice); break;
                default: break;
            }
            
            List<Booking> result = query.Skip(start).Take(length).AsNoTracking().ToList();

            return result;
        }

        public Booking FindById(int idbook)
        {
            Booking result = AppDbContext.Instance.Bookings
                            .Include(x => x.BookingDetails)
                            .ThenInclude(y => y.BoodetIdroomNavigation)
                            .Include(x => x.BookIdclientNavigation)
                            .Include(x => x.BookIduserNavigation)
                            .Where(x => x.IdBook == idbook)
                            .AsNoTracking()
                            .SingleOrDefault();
             return result;     
        }

        public Booking FindForInvoice(string code)
        {
            Booking result = (from book in AppDbContext.Instance.Bookings
                              join client in AppDbContext.Instance.Clients on book.BookIdclient equals client.IdClient
                              join bkdt in AppDbContext.Instance.BookingDetails on book.IdBook equals bkdt.BoodetIdbook
                              where book.BookCode.Equals(code) && book.BookStatus != "Processed"
                              select new Booking()
                              {
                                  IdBook = book.IdBook,
                                  BookBookdate = book.BookBookdate,
                                  BookCheckindate = book.BookCheckindate,
                                  BookCheckoutdate = book.BookCheckoutdate,
                                  BookTotalprice = book.BookTotalprice,
                                  BookDeposit = book.BookDeposit,
                                  BookStatus = book.BookStatus,
                                  BookIdclientNavigation = new Client
                                  {
                                       CliCode = client.CliCode,
                                       CliName =client.CliName,
                                       CliPhone = client.CliPhone,
                                       CliGmail = client.CliGmail
                                  },                                  
                              }
                              ).AsNoTracking().FirstOrDefault();
            return result;
        }
       

        public int Update(Booking booking)
        {
            int trackedID = 0;
            AppDbContext.Instance.Update(booking);
            AppDbContext.Instance.SaveChanges();
            trackedID = booking.IdBook;
            AppDbContext.Instance.Entry(booking).State = EntityState.Detached;
            return trackedID;
        }

   

        public int Add(Booking booking)
        {
            int trackedID = 0;
            AppDbContext.Instance.Add(booking);
            AppDbContext.Instance.SaveChanges();
            trackedID = booking.IdBook;
            AppDbContext.Instance.Entry(booking).State = EntityState.Detached;
            return trackedID;

        }

        public void Delete(int idbook)
        {
            Booking result = AppDbContext.Instance.Bookings
                           .Include(x => x.BookingDetails)
                           .ThenInclude(y => y.BoodetIdroomNavigation)
                           .Include(x => x.BookIdclientNavigation)
                           .Include(x => x.BookIduserNavigation)
                           .Where(x => x.IdBook == idbook)
                           .SingleOrDefault();

            Booking booking = AppDbContext.Instance.Bookings.Where(x =>x.IdBook == idbook).Include(x => x.BookingDetails).SingleOrDefault();
            AppDbContext.Instance.RemoveRange(booking.BookingDetails);
            AppDbContext.Instance.SaveChanges();
            AppDbContext.Instance.Remove(booking);
            AppDbContext.Instance.SaveChanges();
            AppDbContext.Instance.Entry(booking).State = EntityState.Detached;
        }
       

        

        public void CheckinBooking(int idbook)
        {
            Booking booking = AppDbContext.Instance.Bookings.Where(x =>x.IdBook == idbook).FirstOrDefault();           
            booking.BookStatus = "Checkin";
            AppDbContext.Instance.Bookings.Update(booking);
            AppDbContext.Instance.SaveChanges();
            AppDbContext.Instance.Entry(booking).State = EntityState.Detached;
        }

        public int GetTotalRow(CalendarVM searchByDate, string search , string status)
        {
            int totalrows = 0;
            var predicate = PredicateBuilder.True<Booking>();
            if (search != "")
                predicate = predicate.And(x => x.BookCode.Contains(search) || x.BookIdclientNavigation.CliCode.Contains(search) || x.BookIduserNavigation.UserCode.Contains(search));
            if (searchByDate.type.Equals("Booked Date"))
                predicate = predicate.And(x => x.BookBookdate >= searchByDate.fromDate && x.BookBookdate <= searchByDate.toDate);
            if (searchByDate.type.Equals("Due Date"))
                predicate = predicate.And(x => x.BookDuedate >= searchByDate.fromDate && x.BookDuedate <= searchByDate.toDate);
            if (searchByDate.type.Equals("Checkin Date"))
                predicate = predicate.And(x => x.BookCheckindate >= searchByDate.fromDate && x.BookCheckindate <= searchByDate.toDate);
            if (searchByDate.type.Equals("Checkout Date"))
                predicate = predicate.And(x => x.BookCheckoutdate >= searchByDate.fromDate && x.BookCheckoutdate <= searchByDate.toDate);
            if (status != "All")
                predicate = predicate.And(x => x.BookStatus == status);

           totalrows = AppDbContext.Instance.Bookings
                              .Where(predicate).Count();
            return totalrows;
        }


        public int GetNextId()
        {
            int id;
            using (var command = AppDbContext.Instance.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT IDENT_CURRENT('booking')+1";
                AppDbContext.Instance.Database.OpenConnection();
                using (var result = command.ExecuteReader())
                {
                    result.Read();
                    id = Decimal.ToInt32((decimal)result[0]);
                }
            }
            return id;
        }

        public List<Booking> GetAll()
        {
            throw new NotImplementedException();
        }

        public void Restore(int id)
        {
            throw new NotImplementedException();
        }
    }
}
