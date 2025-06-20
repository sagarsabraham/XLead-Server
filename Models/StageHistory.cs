﻿namespace XLead_Server.Models 
{ 
    public class StageHistory 
    { 
        public long Id { get; set; } 
        public long DealStageId { get; set; } 
        public DealStage DealStage { get; set; } 
        public long DealId { get; set; } 
        public Deal Deal { get; set; } 
        public long CreatedBy { get; set; } 
        public User Creator { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    } 
}