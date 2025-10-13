-- Paddle Price ID'lerini güncelle
-- Basic Plan ($45/month)
UPDATE SubscriptionPlans SET PaddlePriceId = 'pri_01k7akm9vxby21janvh9av3ajf' WHERE Id = 1;

-- Professional Plan ($75/month)
UPDATE SubscriptionPlans SET PaddlePriceId = 'pri_01k7aknda27yemng3tdama2pgs' WHERE Id = 2;

-- Premium Plan ($125/month)
UPDATE SubscriptionPlans SET PaddlePriceId = 'pri_01k7akq2ynww0yhrz6ypf22890' WHERE Id = 3;

-- Kontrol için
SELECT Id, Name, Price, Currency, PaddlePriceId FROM SubscriptionPlans;
