document.addEventListener("DOMContentLoaded", function () {
    const svg = d3.select("svg"),
        width = +svg.attr("width"),
        height = +svg.attr("height");

    const treeLayout = d3.tree().size([height, width - 160]);

    const g = svg.append("g")
        .attr("transform", "translate(80,0)");

    d3.json("data.json").then(data => {
        const root = d3.hierarchy(data);
        treeLayout(root);

        g.selectAll('.link')
            .data(root.links())
            .enter().append('path')
            .attr("class", "link")
            .attr("d", d3.linkHorizontal()
                .x(d => d.y)
                .y(d => d.x));

        g.selectAll('.node')
            .data(root.descendants())
            .enter().append('g')
            .attr("class", "node")
            .attr("transform", d => `translate(${d.y},${d.x})`)
            .append('text')
            .attr("dy", "0.35em")
            .attr("x", d => d.children ? -8 : 8)
            .style("text-anchor", d => d.children ? "end" : "start")
            .text(d => d.data.name);
    });
});
