-- Add BookingId column to FormBooking to link a row to a Booking (dbo.Booking / CONTAINEROUTBOUNDNOTIFY_sale)
USE [NVOAMASIS]
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[FormBooking]') AND name = 'BookingId'
)
BEGIN
    ALTER TABLE [dbo].[FormBooking] ADD [BookingId] [uniqueidentifier] NULL;
END
GO
