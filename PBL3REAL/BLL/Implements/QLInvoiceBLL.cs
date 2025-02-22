﻿using AutoMapper;
using HotelManagement.DAL.Implement;
using PBL3REAL.BLL.Interfaces;
using PBL3REAL.DAL;
using PBL3REAL.DAL.FacadeDAL;
using PBL3REAL.Model;
using PBL3REAL.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace PBL3REAL.BLL
{
    public class QLInvoiceBLL : IInvoiceBLL
    {
        private Mapper mapper;
        private InvoiceDALManageFacade _invoiceDALManageFacade; 
        public QLInvoiceBLL()
        {
            mapper = new Mapper(MapperVM.config);
            _invoiceDALManageFacade = new InvoiceDALManageFacade();
        }

        public InvoiceDetailVM InfoAddInvoice(string bookCode)
        {
            try
            {
                Booking booking = _invoiceDALManageFacade.FindBookingForInvoice(bookCode);
                if (booking == null) throw new ArgumentException("Wrong Code");
                InvoiceDetailVM invoiceDetailVM = new InvoiceDetailVM
                {
                    InvIdbook = booking.IdBook,
                    BookBookDate = booking.BookBookdate,
                    BookCheckindate = booking.BookCheckindate,
                    BookChecoutdate = booking.BookCheckoutdate,
                    CliCode = booking.BookIdclientNavigation.CliCode,
                    CliName = booking.BookIdclientNavigation.CliName,
                    CliPhone = booking.BookIdclientNavigation.CliPhone,
                    CliGmail = booking.BookIdclientNavigation.CliGmail,
                };
                if (booking.BookStatus == "Checkin")
                {
                    invoiceDetailVM.TotalPrice = booking.BookTotalprice;
                    invoiceDetailVM.InvStatus = "Total";
                }
                else
                {
                    invoiceDetailVM.TotalPrice = booking.BookDeposit;
                    invoiceDetailVM.InvStatus = "Deposit";
                }
                int durationDate = invoiceDetailVM.BookChecoutdate.Subtract(invoiceDetailVM.BookCheckindate).Days;
                foreach (Room room in _invoiceDALManageFacade.FindRoomByIdBook(booking.IdBook))
                {
                    invoiceDetailVM.ListRoom.Add(new Invoice_RoomVM
                    {
                        Name = room.RoomName,
                        Price = room.RoomIdroomtypeNavigation.RotyCurrentprice,
                        RoomType = room.RoomIdroomtypeNavigation.RotyName,
                        Duration = durationDate,
                        Amount = room.RoomIdroomtypeNavigation.RotyCurrentprice * durationDate

                    });
                }
                return invoiceDetailVM;
            }
            catch (Exception)
            {
                throw; 
            }
        }

        public void AddInvoice(InvoiceDetailVM invoiceDetailVM)
        {
            Invoice invoice = new Invoice();
            mapper.Map(invoiceDetailVM, invoice);
            try
            {
                _invoiceDALManageFacade.AddInvoice(invoice);
            }
            catch(Exception e)
            {

            }
    
        }

        public void DeleteInvoice(int  idInvoice)
        {
            _invoiceDALManageFacade.DeleteInvoice(idInvoice);
        }

        public InvoiceDetailVM GetDetail(int idinvoice)
        {
            try
            {                
                Invoice invoice = _invoiceDALManageFacade.FindInvoiceById(idinvoice);
                InvoiceDetailVM invoiceDetailVM = mapper.Map<InvoiceDetailVM>(invoice);
                Booking booking = invoice.InvIdbookNavigation;
                invoiceDetailVM.BookCheckindate = booking.BookCheckindate;
                invoiceDetailVM.BookChecoutdate = booking.BookCheckoutdate;
                invoiceDetailVM.BookBookDate = booking.BookBookdate;
                invoiceDetailVM.BookCode = booking.BookCode;
                invoiceDetailVM.CliName = booking.BookIdclientNavigation.CliName;
                invoiceDetailVM.CliCode = booking.BookIdclientNavigation.CliCode;
                invoiceDetailVM.CliPhone = booking.BookIdclientNavigation.CliPhone;
                invoiceDetailVM.CliGmail = booking.BookIdclientNavigation.CliGmail;
                invoiceDetailVM.UserCode = booking.BookIduserNavigation.UserCode;

                int durationDate = invoiceDetailVM.BookChecoutdate.Subtract(invoiceDetailVM.BookCheckindate).Days;
                foreach (Room room in _invoiceDALManageFacade.FindRoomByIdBook(invoice.InvIdbookNavigation.IdBook))
                {
                    invoiceDetailVM.ListRoom.Add(new Invoice_RoomVM
                    {
                        Name = room.RoomName,
                        Price = room.RoomIdroomtypeNavigation.RotyCurrentprice,
                        RoomType = room.RoomIdroomtypeNavigation.RotyName,
                        Duration = durationDate,
                        Amount = room.RoomIdroomtypeNavigation.RotyCurrentprice * durationDate

                    });
                }
                return invoiceDetailVM;        
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<InvoiceVM>FindByProperties(int pages , int rows ,string bookCode, string invCode , string orderBy , CalendarVM searchByDate)
        {
            int start = (pages - 1) * rows;
            int length = rows;
            List<InvoiceVM> listVm = new List<InvoiceVM>();
            foreach (var value in _invoiceDALManageFacade.FindInvoice(start, length, bookCode, invCode, searchByDate, orderBy))
            {
                InvoiceVM invoiceVM = mapper.Map<InvoiceVM>(value);
                invoiceVM.Index = ++start;
                listVm.Add(invoiceVM);
            }
            return listVm;
        }

        public int GetPagination(int rows, string code , CalendarVM searchByDate)
        {
            int totalRows = _invoiceDALManageFacade.GetInvoiceTotalRow(code, searchByDate);
            int totalpage;
            if (totalRows % rows == 0)
            {
                totalpage = totalRows / rows;
            }
            else
            {
                totalpage = totalRows / rows + 1;
            }
            return totalpage;
        }

        public List<Statistic1> FindForStatistic(DateTime fromDate, DateTime toDate)
        {
            List<Statistic1> listVM = null;
            try
            {
                listVM = _invoiceDALManageFacade.FindForStatistic(fromDate, toDate);
            }
            catch (Exception)
            {

            }
            return listVM;
        }

        public List<Statistic2>FindForStatistic2 (DateTime fromDate, DateTime toDate)
        {
            return _invoiceDALManageFacade.FindForStatistic2(fromDate, toDate);
        }
        
        public string AnalyzingIncome(List<Statistic1> statistic1s, DateTime from, DateTime to)
        {
            if (statistic1s == null) { return ""; }
            if (statistic1s == null) { return ""; }
            string res1 = "- Ngày có doanh thu cao nhất: ";
            string res2 = "- Ngày có doanh thu thấp nhất: ";
            string res3 = "- Doanh thu trung bình: ";
            string res4 = "- Số ngày không có doanh thu: ";
            string res5 = "- Số ngày không có doanh thu liên tiếp: ";
            int Max_Income = 0, Min_Income = statistic1s[0].TotalPriceByDate, Total_Day = Convert.ToInt32((to.Date - from.Date).TotalDays),
                Day_Non_Income = Total_Day - statistic1s.Count,Day_Consecutive_Non_Income = 0, Day_Consecutive_Non_Income_Temp = 0;
            double Total_Income = 0, Avg_Income = 0;
            DateTime PrevDay = DateTime.Now;
            foreach (Statistic1 statistic1 in statistic1s)
            {
                if (statistic1.TotalPriceByDate > Max_Income) { Max_Income = statistic1.TotalPriceByDate; }
                if (statistic1.TotalPriceByDate < Min_Income) { Min_Income = statistic1.TotalPriceByDate; }
                Total_Income += statistic1.TotalPriceByDate;
                if (statistic1s.IndexOf(statistic1) != 0)
                {   
                    Day_Consecutive_Non_Income_Temp = Convert.ToInt32((statistic1.Date - PrevDay.Date).TotalDays);
                    if (Day_Consecutive_Non_Income_Temp > 1)
                    {
                        Day_Consecutive_Non_Income_Temp -= 1;
                    }
                    if (Day_Consecutive_Non_Income < Day_Consecutive_Non_Income_Temp)
                    {
                        Day_Consecutive_Non_Income = Day_Consecutive_Non_Income_Temp;
                        Day_Consecutive_Non_Income_Temp = 0;
                    }
                }
                if (statistic1s.IndexOf(statistic1) == statistic1s.Count - 1 && statistic1.Date < to)
                {
                    Day_Consecutive_Non_Income_Temp = Convert.ToInt32((to.Date - statistic1.Date).TotalDays) - 1;
                    if (Day_Consecutive_Non_Income_Temp > Day_Consecutive_Non_Income)
                    {
                        Day_Consecutive_Non_Income = Day_Consecutive_Non_Income_Temp;
                    }
                    break;
                }
                PrevDay = statistic1.Date;
            }
            Avg_Income = Total_Income / Total_Day;
            res1 += (Max_Income.ToString() + " VND");
            res2 += (Min_Income.ToString() + " VND");
            res3 += (Avg_Income.ToString() + " VND");
            res4 += (Day_Non_Income.ToString() + " Ngày");
            res5 += (Day_Consecutive_Non_Income.ToString() + " Ngày");
            return res1 + "\n" + res2 + "\n" + res3 + "\n" + res4 + "\n" + res5;
        }
    }
}
