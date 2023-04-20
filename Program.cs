using System;
using System.Collections.Generic;
using System.Threading;

namespace Flaskeautomaten
{
    public class Program
    {
        static int _maxBufferSize = 10;

        // Create queues to hold items produced by the Producer and split by the Splitter
        static Queue<string> _beerBox = new Queue<string>();
        static Queue<string> _sodaBox = new Queue<string>();
        static Queue<string> _itemBox = new Queue<string>();

        public static void Main(string[] args)
        {
            Program program = new Program();
            // Create a producer, a splitter, and two consumer threads
            Thread producerThread = new
           Thread(new ThreadStart(program.Producer));
            Thread splitterThread = new
            Thread(new ThreadStart(program.Splitter));
            Thread beerConsumerThread = new
            Thread(new ThreadStart(program.BeerConsumer));
            Thread sodaConsumerThread = new
            Thread(new ThreadStart(program.SodaConsumer));

            producerThread.Start();
            splitterThread.Start();
            beerConsumerThread.Start();
            sodaConsumerThread.Start();

            producerThread.Join();
            splitterThread.Join();
            beerConsumerThread.Join();
            sodaConsumerThread.Join();
        }

        // A method to produce items
        public void Producer()
        {
            int counter = 1;
            while (true)
            {
                Monitor.Enter(_itemBox);
                try
                {
                    // Produce a beer or soda item and add it to the queue
                    string item = (counter % 2 == 0) ? "øl" : "sodavand";
                    item += counter.ToString();
                    _itemBox.Enqueue(item);
                    Console.WriteLine($"Producer har produceret: {item}");
                    counter++;

                    // Random delay 
                    Random rnd = new Random();
                    int number = rnd.Next(1, 2000);
                    Thread.Sleep(number);

                    // Signal the waiting threads that there are items in the queue
                    if (_itemBox.Count > 0)
                    {
                        Console.WriteLine("Producer waits ");
                        Monitor.PulseAll(_itemBox);
                    }
                }
                finally
                {
                    Monitor.Exit(_itemBox);
                }
            }
        }

        // A method to split items into two queues
        public void Splitter()
        {
            while (true)
            {
                Monitor.Enter(_itemBox);
                try
                {
                    // If the _itemBox is not empty, split the items into two queues
                    if (_itemBox.Count > 0)
                    {
                        string item = _itemBox.Dequeue();

                        // Check if the item is a beer or soda, and enqueue it in the respective queue
                        if (item.StartsWith("øl"))
                        {
                            Monitor.Enter(_beerBox);
                            try
                            {
                                // If the beer buffer is full, wait until there are spaces in the queue
                                while (_beerBox.Count == _maxBufferSize)
                                {
                                    Monitor.Wait(_beerBox);
                                }
                                _beerBox.Enqueue(item);
                                Console.WriteLine($"Splitter har sendt en flaske øl til BeerConsumer");
                                // Signal the waiting threads that there are items in the queue
                                if (_beerBox.Count > 0)
                                {
                                    Console.WriteLine("Splitter waits ");
                                    Monitor.PulseAll(_beerBox);
                                }
                            }
                            finally
                            {
                                Monitor.Exit(_beerBox);
                            }
                        }
                        else
                        {
                            Monitor.Enter(_sodaBox);
                            try
                            {
                                // If the soda buffer is full, wait until there are spaces in the queue
                                while (_sodaBox.Count == _maxBufferSize)
                                {
                                    Monitor.Wait(_sodaBox);
                                }
                                _sodaBox.Enqueue(item);
                                Console.WriteLine($"Splitter har sendt en flaske sodavand til SodaConsumer");
                                // Signal the waiting threads that there are items in the queue
                                if (_sodaBox.Count > 0)
                                {
                                    Console.WriteLine("Splitter waits ");
                                    Monitor.PulseAll(_sodaBox);
                                }
                            }
                            finally
                            {
                                Monitor.Exit(_sodaBox);
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_itemBox);
                }
            }
        }

        // A method to consume soda items
        public void SodaConsumer()
        {
            while (true)
            {
                Monitor.Enter(_sodaBox);
                try
                {
                    // If the soda buffer is empty, wait until there are items in the queue
                    while (_sodaBox.Count == 0)
                    {
                        Monitor.Wait(_sodaBox);
                    }
                    // Remove a soda item from the queue and consume it
                    string item = _sodaBox.Dequeue();
                    Console.WriteLine($"SodaConsumer har consumeret: {item}");
                    // If the soda buffer is empty, signal the waiting threads
                    if (_sodaBox.Count == 0)
                    {
                        Console.WriteLine("SodaConsumer waits ");
                    }
                    // Random delay
                    Random rnd = new Random();
                    int number = rnd.Next(1, 2000);
                    Thread.Sleep(number);

                    // Signal the waiting threads that there are spaces in the queue
                    if (_sodaBox.Count < _maxBufferSize)
                    {
                        Console.WriteLine("SodaConsumer waits ");
                        Monitor.PulseAll(_sodaBox);
                    }
                }
                finally
                {
                    Monitor.Exit(_sodaBox);
                }
            }
        }

        // A method to consume beer items
        public void BeerConsumer()
        {
            while (true)
            {
                Monitor.Enter(_beerBox);
                try
                {
                    // If the beer buffer is empty, wait until there are items in the queue
                    while (_beerBox.Count == 0)
                    {
                        Monitor.Wait(_beerBox);
                    }
                    // Remove a beer item from the queue and consume it
                    string item = _beerBox.Dequeue();
                    Console.WriteLine($"BeerConsumer har consumeret: {item}");
                    // If the beer buffer is empty, signal the waiting threads
                    if (_beerBox.Count == 0)
                    {
                        Console.WriteLine("BeerConsumer waits ");
                    }
                    // Random delay
                    Random rnd = new Random();
                    int number = rnd.Next(1, 2000);
                    Thread.Sleep(number);

                    // Signal the waiting threads that there are spaces in the queue
                    if (_beerBox.Count < _maxBufferSize)
                    {
                        Console.WriteLine("BeerConsumer waits ");
                        Monitor.PulseAll(_beerBox);
                    }
                }
                finally
                {
                    Monitor.Exit(_beerBox);
                }
            }
        }
    }
}