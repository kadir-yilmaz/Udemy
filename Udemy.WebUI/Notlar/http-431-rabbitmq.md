# HTTP 431 - RabbitMQ Management UI Erişim Hatası

## Sorun
RabbitMQ Management UI'ye (`localhost:15672`) erişmeye çalışırken:
```
HTTP ERROR 431 - Request Header Fields Too Large
```

## Sebep
Tarayıcıdaki cookie'ler veya cache çok büyümüş, request header limiti aşılmış.

## Çözüm
**Incognito/Gizli Sekme** ile açınca sorun çözüldü.

### Alternatif Çözümler:
- `Ctrl + Shift + Delete` ile cookie/cache temizleme
- Farklı tarayıcı kullanma
- Browser DevTools > Application > Cookies'den ilgili domain'i silme
