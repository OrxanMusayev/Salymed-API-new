-- ===================================================================
-- Paddle Price ID Güncelleme
-- ===================================================================
-- 
-- DİKKAT: Bu SQL'i çalıştırmadan önce Paddle Dashboard'dan 
-- doğru Price ID'leri almanız gerekmektedir!
--
-- Paddle'da "pri_" ile başlayan ID'ler PRICE ID'leridir.
-- "pro_" ile başlayan ID'ler PRODUCT ID'leridir - bu yanlıştır!
--
-- Örnek Price ID: pri_01k7c65ab1m1gv1s0ghvrvt4fa
-- ===================================================================

-- Basic Plan için (ID = 1)
-- Paddle Dashboard > Catalog > Products > Basic Plan > Prices
UPDATE SubscriptionPlans 
SET PaddlePriceId = 'pri_01hxxx...' -- BURAYA PADDLE'DAN ALDIĞINIZ GERÇEK PRICE ID'Yİ YAPIN!
WHERE Id = 1 AND Name LIKE '%Basic%';

-- Professional Plan için (ID = 2)
-- Paddle Dashboard > Catalog > Products > Professional Plan > Prices
UPDATE SubscriptionPlans 
SET PaddlePriceId = 'pri_01hyyy...' -- BURAYA PADDLE'DAN ALDIĞINIZ GERÇEK PRICE ID'Yİ YAPIN!
WHERE Id = 2 AND Name LIKE '%Professional%';

-- Enterprise Plan için (ID = 3)
-- Paddle Dashboard > Catalog > Products > Enterprise Plan > Prices
UPDATE SubscriptionPlans 
SET PaddlePriceId = 'pri_01hzzz...' -- BURAYA PADDLE'DAN ALDIĞINIZ GERÇEK PRICE ID'Yİ YAPIN!
WHERE Id = 3 AND Name LIKE '%Enterprise%';

-- ===================================================================
-- Kontrol - Price ID'lerin doğru şekilde ayarlandığını kontrol edin
-- ===================================================================
SELECT 
    Id, 
    Name, 
    Price, 
    Currency, 
    PaddlePriceId,
    CASE 
        WHEN PaddlePriceId IS NULL THEN '❌ EKSİK'
        WHEN PaddlePriceId LIKE 'pri_%' THEN '✅ DOĞRU'
        WHEN PaddlePriceId LIKE 'pro_%' THEN '❌ YANLIŞ (Product ID kullanılmış, Price ID olmalı)'
        ELSE '⚠️ BİLİNMEYEN FORMAT'
    END AS Status
FROM SubscriptionPlans
ORDER BY Id;

-- ===================================================================
-- Eğer hala Product ID kullanıyorsanız temizleyin
-- ===================================================================
-- UPDATE SubscriptionPlans SET PaddlePriceId = NULL WHERE PaddlePriceId LIKE 'pro_%';
