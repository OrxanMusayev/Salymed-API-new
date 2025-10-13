-- Test için product ID kullanıyoruz (normalde price ID olmalı)
-- Paddle dashboard'da her plan için price oluşturduktan sonra bunları güncelleyin

UPDATE SubscriptionPlans SET PaddlePriceId = 'pro_01k7akgks94hd1pnz42f76z0a6' WHERE Id = 1;
UPDATE SubscriptionPlans SET PaddlePriceId = 'pro_01k7akgks94hd1pnz42f76z0a6' WHERE Id = 2;
UPDATE SubscriptionPlans SET PaddlePriceId = 'pro_01k7akgks94hd1pnz42f76z0a6' WHERE Id = 3;

-- Kontrol için
SELECT Id, Name, Price, Currency, PaddlePriceId FROM SubscriptionPlans;
