﻿using ErpNet.FP.Core;
using ErpNet.FP.Server.Models;
using System;

namespace ErpNet.FP.Server.Contexts
{
    public enum PrintJobAction
    {
        None,
        Receipt,
        ReversalReceipt,
        Withdraw,
        Deposit,
        XReport,
        ZReport,
        SetDateTime
    }

    public delegate object Run(object document);

    public class PrintJob
    {
        public const int DefaultTimeout = 29000; // 29 seconds

        public DateTime Enqueued = DateTime.Now;
        public DateTime Started = DateTime.MaxValue;
        public DateTime Finished = DateTime.MaxValue;

        public PrintJobAction Action = PrintJobAction.None;
        public IFiscalPrinter? Printer;
        public TaskStatus TaskStatus = TaskStatus.Unknown;
        public object? Document;
        public object? Result;

        public void Run()
        {
            if (Printer == null) return;
            Started = DateTime.Now;
            TaskStatus = TaskStatus.Running;
            switch (Action)
            {
                case PrintJobAction.Receipt:
                    if (Document != null)
                    {
                        var (info, status) = Printer.PrintReceipt((Receipt)Document);
                        Result = new PrintReceiptResult
                        {
                            Info = info,
                            Status = status
                        };
                    }
                    break;
                case PrintJobAction.ReversalReceipt:
                    if (Document != null)
                    {
                        Result = new PrintResult
                        {
                            Status = Printer.PrintReversalReceipt((ReversalReceipt)Document)
                        };
                    };
                    break;
                case PrintJobAction.Withdraw:
                    if (Document != null)
                    {
                        Result = new PrintResult
                        {
                            Status = Printer.PrintMoneyWithdraw(((TransferAmount)Document).Amount)
                        };
                    }
                    break;
                case PrintJobAction.Deposit:
                    if (Document != null)
                    {
                        Result = new PrintResult
                        {
                            Status = Printer.PrintMoneyDeposit(((TransferAmount)Document).Amount)
                        };
                    }
                    break;
                case PrintJobAction.XReport:
                    Result = new PrintResult
                    {
                        Status = Printer.PrintXReport()
                    };
                    break;
                case PrintJobAction.ZReport:
                    Result = new PrintResult
                    {
                        Status = Printer.PrintZReport()
                    };
                    break;
                case PrintJobAction.SetDateTime:
                    if (Document != null)
                    {
                        Result = new PrintResult
                        {
                            Status = Printer.SetDateTime(((CurrentDateTime)Document).DateTime)
                        };
                    }
                    break;
                default:
                    break;
            }
            Finished = DateTime.Now;
            TaskStatus = TaskStatus.Finished;
        }
    }
}
