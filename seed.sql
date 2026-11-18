-- Insert Course
INSERT INTO portfolio.course
    (course_code, course_title, description)
VALUES
    (
        'DBMWEB1',
        'Database Management and Web Systems',
        'A course focused on database management and web development.'
    );

-- Insert Project
INSERT INTO portfolio.project
    (course_id, project_title, description, video_url)
VALUES
    (
        (SELECT course_id
         FROM portfolio.course
         WHERE course_code = 'DBMWEB1'),
        'My Portfolio Website',
        'My Portfolio Website is a web-based project developed through coding and website design using ASP.NET Core MVC, C#, HTML, CSS, and Supabase. It showcases my academic courses, projects, and multimedia works. Visitors can explore my projects, view images and videos, and learn about my skills, creativity, and learning experiences as an Information Technology student. It also includes an editor section where course and project information can be added, updated, and deleted.',
        NULL
    );