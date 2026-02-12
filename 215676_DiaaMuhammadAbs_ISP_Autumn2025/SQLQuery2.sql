USE Art;

INSERT INTO Categories (Name, UrlSegment, Icon, DisplayOrder)
VALUES
(N'Special Offers', N'special-offers', N'fa-solid fa-star', 1),
(N'Art tools', N'art-tools', N'fa-solid fa-palette', 2),
(N'Pens & Markers', N'pens-markers', N'fa-solid fa-pen', 3),
(N'Paints & Color', N'paints-color', N'fa-solid fa-palette', 4),
(N'Varnishes and Cleaners', N'varnishes-cleaners', N'fa-solid fa-spray-can', 5),
(N'Brushes', N'brushes', N'fa-solid fa-paint-brush', 6),
(N'Stands', N'stands', N'fa-solid fa-chalkboard', 7),
(N'Sketch pads & Canvas', N'sketch-canvas', N'fa-solid fa-book-open', 8),
(N'Art work', N'art-work', N'fa-solid fa-image', 9),
(N'Art Books', N'art-books', N'fa-solid fa-book', 10);

SELECT Id, Name, Icon FROM Categories;


