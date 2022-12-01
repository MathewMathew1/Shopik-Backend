## Technologies used:
  Net Core(c#)  
  MongoDb  
  Docker  
  
# About Project:  
  Ecomerce site selling difrent kind of clothes.   
  Backend created with C#Net.  
  Docker used to help with building in develompent and production.  
  Igmur used to store images of products.  
  StripeApi used to process payments.  
    On request to create order backend will create a link to stripe and send it back to client.    
    Once client will confirm payment, stripe using webhook will hit an endpoint to confirm that transcation was sucessfull making order confirmed in database.  
    Then verified user can later confirm that order was transported to corect adress corectly.  
  
